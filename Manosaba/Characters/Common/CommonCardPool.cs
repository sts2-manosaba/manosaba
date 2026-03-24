using BaseLib.Abstracts;
using Godot;
using manosaba.Extensions;
using Manosaba.Characters.Common.Cards;
using MegaCrit.Sts2.Core.Models;

namespace manosaba.Characters.Common;

public class CommonCardPool : CustomCardPoolModel
{
    public override string Title => "manosaba_common"; //This is not a display name.

    public override string BigEnergyIconPath => "charui/big_energy.png".ImagePath();
    public override string TextEnergyIconPath => "charui/text_energy.png".ImagePath();


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
    public override Color DeckEntryCardColor => new("FFFFFF");

    public override bool IsColorless => true;

    public static CardModel[] getAllCards()
    {
        return new CardModel[17] {
            ModelDb.Card<PicnicTime>(),
            ModelDb.Card<TrialStart>(),
            ModelDb.Card<Vote>(),
            ModelDb.Card<ConcentratedFire>(),
            ModelDb.Card<Hotpot>(),
            ModelDb.Card<Boulders>(),
            ModelDb.Card<Suicide>(),
            ModelDb.Card<Alibi>(),
            ModelDb.Card<SimpleSpear>(),
            ModelDb.Card<Electrocution>(),
            ModelDb.Card<Bullseye>(),
            ModelDb.Card<DriftApart>(),
            ModelDb.Card<ForgeSpear>(),
            ModelDb.Card<SSArrow>(),
            ModelDb.Card<SSBroom>(),
            ModelDb.Card<SSRapier>(),
            ModelDb.Card<SSRibbon>()
        };
    }

}