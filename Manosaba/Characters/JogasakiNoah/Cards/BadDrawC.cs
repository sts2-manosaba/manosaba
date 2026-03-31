using BaseLib.Utils;
using manosaba.Characters.JogasakiNoah;

namespace Manosaba.Characters.JogasakiNoah.Cards;

[Pool(typeof(JogasakiNoahCardPool))]
public class BadDrawC : BadDrawBase
{
    protected override Type[] SiblingTypes => [typeof(BadDrawA), typeof(BadDrawB), typeof(BadDrawC)];
}
