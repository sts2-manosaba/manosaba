Custom character select sounds (Godot AudioStream, not FMOD).

Place one file per character, named exactly:
  {character_id}_select.ogg
  or {character_id}_select.wav
  or {character_id}_select.mp3

character_id matches each class's CharacterId constant, e.g. tono_hanna -> tono_hanna_select.ogg

Open the Godot project once so imports generate, then export Manosaba.pck.

Loudness: edit CharacterSelectExtraDb in Audio/GodotSfxRouter.cs (+6 dB default; try +9 or +12 if still quiet).

自我介紹在
Audio\Voice\Act01_Chapter01\Act01_Chapter01_Adv04