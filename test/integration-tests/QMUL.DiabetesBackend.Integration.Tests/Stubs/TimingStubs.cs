namespace QMUL.DiabetesBackend.Integration.Tests.Stubs;

using Hl7.Fhir.Model;

public static class TimingStubs
{
    public static Timing FourWeeksDaily => new()
    {
        Repeat = new Timing.RepeatComponent
        {
            Bounds = new Duration
            {
                Value = 4,
                Unit = Timing.UnitsOfTime.Wk.ToString()
            },
            Frequency = 1,
            Period = 1,
            PeriodUnit = Timing.UnitsOfTime.D,
            TimeOfDay = new []{ "10:00" },
            Offset = 0
        }
    };

    public static Timing DatedDuration => new()
    {
        Repeat = new Timing.RepeatComponent
        {
            Bounds = new Period
            {
                Start = "2023-08-01",
                End = "2023-08-10"
            },
            Frequency = 1,
            Period = 1,
            PeriodUnit = Timing.UnitsOfTime.D,
            TimeOfDay = new []{ "10:00" },
            Offset = 0
        }
    };
}