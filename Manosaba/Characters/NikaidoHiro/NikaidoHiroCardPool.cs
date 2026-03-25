using BaseLib.Abstracts;
using Godot;
using manosaba.Characters.Common;
using manosaba.Extensions;
using MegaCrit.Sts2.Core.Models;

namespace manosaba.Characters.NikaidoHiro;

public class NikaidoHiroCardPool : CustomCardPoolModel
{
    public override string Title => NikaidoHiro.CharacterId; //This is not a display name.

    public override string BigEnergyIconPath => "nikaido_hiro_energy.png".CharacterImgPath(NikaidoHiro.CharacterId);
    public override string TextEnergyIconPath => "nikaido_hiro_energy_text.png".CharacterImgPath(NikaidoHiro.CharacterId);


    /* These HSV values will determine the color of your card back.
    They are applied as a shader onto an already colored image,
    so it may take some experimentation to find a color you like.
    Generally they should be values between 0 and 1. */
    public override float H => 1f; //Hue; changes the color.
    public override float S => 1f; //Saturation
    public override float V => 1f; //Brightness

    //Alternatively, leave these values at 1 and provide a custom frame image.
    /*public override Texture2D CustomFrame(CustomCardModel card)
    {
        //This will attempt to load CharMod/images/cards/frame.png
        return PreloadManager.Cache.GetTexture2D("cards/frame.png".ImagePath());
    }*/

    //Color of small card icons
    public override Color DeckEntryCardColor => new("B72222");

    public override bool IsColorless => false;

    protected override CardModel[] GenerateAllCards()
    {
        return ModelDb.CardPool<CommonCardPool>().AllCards.ToArray();
    }
}