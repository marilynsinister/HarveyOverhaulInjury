<frame
  layout="720px 580px"
  background={@Mods/StardewUI/Sprites/MenuBackground}
  border={@Mods/StardewUI/Sprites/MenuBorder}
  border-thickness="36,36,40,36"
  padding="24,20,24,20">
  <lane orientation="vertical">
    <label font="dialogue" text={Title} margin="0,0,4,0" />
    <label font="small" text={InjuryLine} color="#7f6139" margin="0,0,2,0" />
    <label font="small" text={PhaseLine} margin="0,0,2,0" />
    <label font="small" text={ProgressLine} margin="0,0,2,0" />
    <label font="small" text={RegimeStatusLine} margin="0,0,10,0" />
    <label font="small" text={HarveyToneSectionLabel} color="#7f6139" margin="0,0,4,0" />
    <label font="small" text={HarveyToneTitle} color={HarveyToneAccentColor} margin="0,0,2,0" />
    <label font="small" text={HarveyToneDescription} margin="0,0,10,0" />
    <label font="small" text="Сегодня нужно:" color="#7f6139" margin="0,0,4,0" />
    <label font="small" text={TasksText} margin="0,0,10,0" />
    <label font="small" text={TodayFailedSectionText} color="#8b4513" margin="0,0,10,0" />
    <label font="small" text="Почему это важно:" color="#7f6139" margin="0,0,4,0" />
    <label font="small" text={WhyImportant} margin="0,0,10,0" />
    <label font="small" text={ComplicationLine} margin="0,0,10,0" />
    <label font="small" text={HintText} />
  </lane>
</frame>
