using Hl7.Fhir.Utility;

namespace QMUL.DiabetesBackend.Model.Enums
{
    public enum CustomEventTiming
    {
        /// <summary>
      /// Event occurs during the morning. The exact time is unspecified and established by institution convention or patient interpretation.
      /// (system: http://hl7.org/fhir/event-timing)
      /// </summary>
      [EnumLiteral("MORN", "http://hl7.org/fhir/event-timing"), Description("Morning")] MORN,
      /// <summary>
      /// Event occurs during the early morning. The exact time is unspecified and established by institution convention or patient interpretation.
      /// (system: http://hl7.org/fhir/event-timing)
      /// </summary>
      [EnumLiteral("MORN.early", "http://hl7.org/fhir/event-timing"), Description("Early Morning")] MORN_early,
      /// <summary>
      /// Event occurs during the late morning. The exact time is unspecified and established by institution convention or patient interpretation.
      /// (system: http://hl7.org/fhir/event-timing)
      /// </summary>
      [EnumLiteral("MORN.late", "http://hl7.org/fhir/event-timing"), Description("Late Morning")] MORN_late,
      /// <summary>
      /// Event occurs around 12:00pm. The exact time is unspecified and established by institution convention or patient interpretation.
      /// (system: http://hl7.org/fhir/event-timing)
      /// </summary>
      [EnumLiteral("NOON", "http://hl7.org/fhir/event-timing"), Description("Noon")] NOON,
      /// <summary>
      /// Event occurs during the afternoon. The exact time is unspecified and established by institution convention or patient interpretation.
      /// (system: http://hl7.org/fhir/event-timing)
      /// </summary>
      [EnumLiteral("AFT", "http://hl7.org/fhir/event-timing"), Description("Afternoon")] AFT,
      /// <summary>
      /// Event occurs during the early afternoon. The exact time is unspecified and established by institution convention or patient interpretation.
      /// (system: http://hl7.org/fhir/event-timing)
      /// </summary>
      [EnumLiteral("AFT.early", "http://hl7.org/fhir/event-timing"), Description("Early Afternoon")] AFT_early,
      /// <summary>
      /// Event occurs during the late afternoon. The exact time is unspecified and established by institution convention or patient interpretation.
      /// (system: http://hl7.org/fhir/event-timing)
      /// </summary>
      [EnumLiteral("AFT.late", "http://hl7.org/fhir/event-timing"), Description("Late Afternoon")] AFT_late,
      /// <summary>
      /// Event occurs during the evening. The exact time is unspecified and established by institution convention or patient interpretation.
      /// (system: http://hl7.org/fhir/event-timing)
      /// </summary>
      [EnumLiteral("EVE", "http://hl7.org/fhir/event-timing"), Description("Evening")] EVE,
      /// <summary>
      /// Event occurs during the early evening. The exact time is unspecified and established by institution convention or patient interpretation.
      /// (system: http://hl7.org/fhir/event-timing)
      /// </summary>
      [EnumLiteral("EVE.early", "http://hl7.org/fhir/event-timing"), Description("Early Evening")] EVE_early,
      /// <summary>
      /// Event occurs during the late evening. The exact time is unspecified and established by institution convention or patient interpretation.
      /// (system: http://hl7.org/fhir/event-timing)
      /// </summary>
      [EnumLiteral("EVE.late", "http://hl7.org/fhir/event-timing"), Description("Late Evening")] EVE_late,
      /// <summary>
      /// Event occurs during the night. The exact time is unspecified and established by institution convention or patient interpretation.
      /// (system: http://hl7.org/fhir/event-timing)
      /// </summary>
      [EnumLiteral("NIGHT", "http://hl7.org/fhir/event-timing"), Description("Night")] NIGHT,
      /// <summary>
      /// Event occurs [offset] after subject goes to sleep. The exact time is unspecified and established by institution convention or patient interpretation.
      /// (system: http://hl7.org/fhir/event-timing)
      /// </summary>
      [EnumLiteral("PHS", "http://hl7.org/fhir/event-timing"), Description("After Sleep")] PHS,
      /// <summary>
      /// Description: Prior to beginning a regular period of extended sleep (this would exclude naps).  Note that this might occur at different times of day depending on a person's regular sleep schedule.
      /// (system: http://terminology.hl7.org/CodeSystem/v3-TimingEvent)
      /// </summary>
      [EnumLiteral("HS", "http://terminology.hl7.org/CodeSystem/v3-TimingEvent"), Description("HS")] HS,
      /// <summary>
      /// Description: Upon waking up from a regular period of sleep, in order to start regular activities (this would exclude waking up from a nap or temporarily waking up during a period of sleep)
      /// 
      /// 
      ///                            Usage Notes: e.g.
      /// 
      ///                         Take pulse rate on waking in management of thyrotoxicosis.
      /// 
      ///                         Take BP on waking in management of hypertension
      /// 
      ///                         Take basal body temperature on waking in establishing date of ovulation
      /// (system: http://terminology.hl7.org/CodeSystem/v3-TimingEvent)
      /// </summary>
      [EnumLiteral("WAKE", "http://terminology.hl7.org/CodeSystem/v3-TimingEvent"), Description("WAKE")] WAKE,
      /// <summary>
      /// Description: meal (from lat. ante cibus)
      /// (system: http://terminology.hl7.org/CodeSystem/v3-TimingEvent)
      /// </summary>
      [EnumLiteral("C", "http://terminology.hl7.org/CodeSystem/v3-TimingEvent"), Description("C")] C,
      /// <summary>
      /// Description: breakfast (from lat. cibus matutinus)
      /// (system: http://terminology.hl7.org/CodeSystem/v3-TimingEvent)
      /// </summary>
      [EnumLiteral("CM", "http://terminology.hl7.org/CodeSystem/v3-TimingEvent"), Description("CM")] CM,
      /// <summary>
      /// Description: lunch (from lat. cibus diurnus)
      /// (system: http://terminology.hl7.org/CodeSystem/v3-TimingEvent)
      /// </summary>
      [EnumLiteral("CD", "http://terminology.hl7.org/CodeSystem/v3-TimingEvent"), Description("CD")] CD,
      /// <summary>
      /// Description: dinner (from lat. cibus vespertinus)
      /// (system: http://terminology.hl7.org/CodeSystem/v3-TimingEvent)
      /// </summary>
      [EnumLiteral("CV", "http://terminology.hl7.org/CodeSystem/v3-TimingEvent"), Description("CV")] CV,
      /// <summary>
      /// before meal (from lat. ante cibus)
      /// (system: http://terminology.hl7.org/CodeSystem/v3-TimingEvent)
      /// </summary>
      [EnumLiteral("AC", "http://terminology.hl7.org/CodeSystem/v3-TimingEvent"), Description("AC")] AC,
      /// <summary>
      /// before breakfast (from lat. ante cibus matutinus)
      /// (system: http://terminology.hl7.org/CodeSystem/v3-TimingEvent)
      /// </summary>
      [EnumLiteral("ACM", "http://terminology.hl7.org/CodeSystem/v3-TimingEvent"), Description("ACM")] ACM,
      /// <summary>
      /// before lunch (from lat. ante cibus diurnus)
      /// (system: http://terminology.hl7.org/CodeSystem/v3-TimingEvent)
      /// </summary>
      [EnumLiteral("ACD", "http://terminology.hl7.org/CodeSystem/v3-TimingEvent"), Description("ACD")] ACD,
      /// <summary>
      /// before dinner (from lat. ante cibus vespertinus)
      /// (system: http://terminology.hl7.org/CodeSystem/v3-TimingEvent)
      /// </summary>
      [EnumLiteral("ACV", "http://terminology.hl7.org/CodeSystem/v3-TimingEvent"), Description("ACV")] ACV,
      /// <summary>
      /// after meal (from lat. post cibus)
      /// (system: http://terminology.hl7.org/CodeSystem/v3-TimingEvent)
      /// </summary>
      [EnumLiteral("PC", "http://terminology.hl7.org/CodeSystem/v3-TimingEvent"), Description("PC")] PC,
      /// <summary>
      /// after breakfast (from lat. post cibus matutinus)
      /// (system: http://terminology.hl7.org/CodeSystem/v3-TimingEvent)
      /// </summary>
      [EnumLiteral("PCM", "http://terminology.hl7.org/CodeSystem/v3-TimingEvent"), Description("PCM")] PCM,
      /// <summary>
      /// after lunch (from lat. post cibus diurnus)
      /// (system: http://terminology.hl7.org/CodeSystem/v3-TimingEvent)
      /// </summary>
      [EnumLiteral("PCD", "http://terminology.hl7.org/CodeSystem/v3-TimingEvent"), Description("PCD")] PCD,
      /// <summary>
      /// after dinner (from lat. post cibus vespertinus)
      /// (system: http://terminology.hl7.org/CodeSystem/v3-TimingEvent)
      /// </summary>
      [EnumLiteral("PCV", "http://terminology.hl7.org/CodeSystem/v3-TimingEvent"), Description("PCV")] PCV,
      /// <summary>
      /// snack, a meal between lunch and dinner
      /// (system: http://localhost)
      /// </summary>
      [EnumLiteral("SNACK", "http://localhost"), Description("SNACK")] SNACK,
      /// <summary>
      /// At a specific time
      /// (system: http://localhost)
      /// </summary>
      [EnumLiteral("EXACT", "http://localhost"), Description("EXACT")] EXACT,
    }
}