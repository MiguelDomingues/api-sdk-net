﻿namespace Smartling.Api.Model
{
  public class CreateTranslationRequest
  {
    public OriginalAssetKey originalAssetKey { get; set; }
    public CustomTranslationRequestData customOriginalData { get; set; }
    public string title { get; set; }
    public string fileUri { get; set; }
    public string contentHash { get; set; }
    public string originalLocaleId { get; set; }
  }
}