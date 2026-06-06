<frame
  layout="720px 520px"
  background={@Mods/StardewUI/Sprites/MenuBackground}
  border={@Mods/StardewUI/Sprites/MenuBorder}
  border-thickness="36,36,40,36"
  padding="24,20,24,20">
  <lane orientation="vertical">
    <label font="dialogue" text={Title} margin="0,0,8,0" />
    <label font="small" text={PlanTypeLabel} color="#7f6139" margin="0,0,6,0" />
    <label font="small" text={ProgressText} margin="0,0,6,0" />
    <label font="small" text={TodayStatusText} margin="0,0,12,0" />
    <label font="small" text="Правила:" color="#7f6139" margin="0,0,4,0" />
    <label font="small" text={RulesText} margin="0,0,12,0" />
    <label font="small" text={TodayViolationsText} margin="0,0,12,0" />
    <label font="small" text={HintText} />
  </lane>
</frame>
