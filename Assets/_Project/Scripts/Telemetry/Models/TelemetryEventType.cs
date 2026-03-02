namespace BA.Telemetry
{
    public enum TelemetryEventType
    {
        SESSION_START,
        SESSION_END,

        UI_VARIANT_SET,
        UI_SWITCH,

        MODE_START,
        MODE_END,

        ROUND_START,
        ROUND_END,

        TASK_START,
        TASK_END,

        ITEM_INTERACT,
        MATCH_RESULT,
        ARCADE_ACTION,

        RAGE_QUIT,

        DDA_MATCH_CHANGED,
        DDA_ARCADE_CHANGED
    }
}
