# Manosaba 多人崩溃初步分析

分析环境：`Slay the Spire 2 v0.103.2`，游戏主程序集为 Godot 4.5.1 + .NET 9。仓库当前能通过编译，但有大量可空引用警告，其中多人卡牌/Power 是优先排查对象。

## 当前验证结果

- `dotnet build` 通过：`0` errors，约 `282` warnings。
- 自定义大厅难度消息 `ManosabaDifficultySettingsMessage` 会被游戏的 `MessageTypes` 通过 `ReflectionHelper.GetSubtypesInMods<INetMessage>()` 自动发现，表面上不是未注册消息类型问题。
- 多人同步 patch 同时覆盖了 `StartRunLobby` 和 `LoadRunLobby`，但多个状态 reset/freeze 点可能造成 host/client 设置不一致。

## 高风险点 1：非 Player 生物上的 Power 直接使用 `Owner.Player`

这些代码在单人多数时候不明显，但多人/召唤物/宠物/敌方复制效果中，只要 Power 被挂到非玩家 Creature，就会空引用崩溃。

- `Characters/Common/Powers/PrisonPower.cs:20`：`base.Owner.Player.Creature` 未判空。
- `Characters/Common/Powers/MajokaPower.cs:88`：`base.Owner.Player.Creature` 未判空，且在 `AfterPowerAmountChanged` 中触发频繁。
- `Characters/Common/Powers/MajokaPower.cs:105`：`Owner.CombatState.CreateCard(...)` 未确认 `Owner.CombatState` 存在。
- `Characters/Common/Powers/MurderousImpulsePower.cs:55`：`base.Owner.Player.RunState` 未判空。
- `Characters/Common/Powers/MurderousImpulsePower.cs:56`：`Owner.Player.Creature` 未判空。

建议修复模式：先缓存 `Player? ownerPlayer = Owner.Player; Creature? ownerCreature = ownerPlayer?.Creature;`，任一为空就 `return`。

## 高风险点 2：多人卡牌对队友的 `Player` 未判空

`Creature.IsPlayer` 不一定足以保证 `Creature.Player` 对网络同步时机永远非空。编译器也已经提示这些位置可能为空。

- `Characters/SaekiMiria/Cards/MemorySharing.cs:39`：`select c.Player` 可能产出 null。
- `Characters/SaekiMiria/Cards/MemorySharing.cs:45`：`item.Creature.Name` 可能空引用。
- `Characters/SaekiMiria/Cards/SNSExchange.cs:38`：`CardPileCmd.Draw(..., item.Player)` 可能传 null。

建议修复模式：筛选 `c.Player is { Creature: not null } player` 后再使用，不要直接 LINQ `select c.Player`。

## 高风险点 3：`cardPlay.Target` 在 Attack 卡中直接赋值给非空 Creature

多人同步或自动打牌时，`CardPlay.Target` 类型本身就是 `Creature?`。部分卡已手动 `ThrowIfNull`，但仍有未保护位置。

- `Characters/SaekiMiria/Cards/KeyEvidence.cs:42`：`Creature target = cardPlay.Target;` 可空转非空。
- 构建警告中还出现 `DoublePunch.cs`、`LeftPunch.cs`、`CleanUp.cs` 等同类问题。

建议修复模式：对 `AnyEnemy` / `AnyAlly` / `AnyPlayer` 卡牌，在 `OnPlay` 开头使用 `if (cardPlay.Target is not { } target) return;`，避免多人自动/重放路径崩溃。

## 高风险点 4：大厅难度状态生命周期可能不同步

`ManosabaLobbyDifficultyUiHost.OnEnter` 在 `DailyLobbyReady` 分支会再次 `ResetToLobbyDefaults()`：

- `Patches/ManosabaLobbyDifficultyUiHost.cs:104`
- `Patches/ManosabaLobbyDifficultyUiHost.cs:105`

如果客户端在 host 广播后异步进入 daily/custom UI，可能用本地默认值覆盖 host 值，造成 host/client 战斗参数不同。该问题通常表现为 checksum divergence、状态分歧、战斗中断或崩溃。

建议：客户端进入 UI 时不要 `ResetToLobbyDefaults()`；只有 `Singleplayer` / `Host` 首次打开时才重置本地默认值。Client 应等待 host 消息或保持现有 snapshot。

## 高风险点 5：`LoadRunLobby` 的自定义 handler 会立即 Freeze

`Patch_LoadRunLobby_ManosabaDifficultySync.cs:89` 的 handler 收到消息后调用 `FreezeForRunFromHost`。如果消息在本地 `RunState.FromSerializable` 前后时序不同，可能出现：

- 先 freeze host 值，随后本地 `FromSerializable` 又 freeze 本地 lobby snapshot。
- 客户端没收到消息前开始战斗，先以本地默认值创建敌人 HP。

建议：给 `ManosabaLobbyDifficultyState` 加一位 `HasHostSnapshot` 或 `SnapshotSource`，客户端只在收到 host snapshot 后允许 `FreezeForRun()`；否则延迟到 `TryBeginRun`/加载完成前强制等待或保持不改敌人 HP。

## 下一步建议

1. 先修所有 `Owner.Player`、`Owner.Creature`、`cardPlay.Target` 的可空崩溃点。
2. 再修大厅状态：Client 不 reset defaults；Freeze 前区分 host snapshot 与 local defaults。
3. 加日志：在 `ApplyFromHost`、`FreezeForRun`、`CreateForNewRun`、`FromSerializable` 打印 net type 与四个难度值。
4. 用双客户端复现：host 修改难度、client 加入、开始新局、读档重连各测一次。

