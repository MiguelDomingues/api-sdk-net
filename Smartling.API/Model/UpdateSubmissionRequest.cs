﻿namespace Smartling.Api.Model
{
  public class UpdateSubmissionRequest
  {
    public string translationSubmissionUid { get; set; }
    public TargetAssetKey targetAssetKey { get; set; }
    public string customTranslationData { get; set; }
    public string targetLocaleId { get; set; }
    public string state { get; set; }
    public string submitterName { get; set; }
    public int percentComplete { get; set; }
    public object lastExportedDate { get; set; }
    public object lastErrorMessage { get; set; }
  }
}