﻿using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Smartling.Api.Authentication;
using Smartling.Api.Extensions;
using Smartling.Api.Model;

namespace Smartling.Api.Job
{
  public class SubmissionApiClient<TOriginalKey, TCustomRequest, TTargetKey, TCustomSubmission> : ApiClientBase
  {
    private readonly string CreateSubmissionUrl = "/submission-service-api/v2/projects/{0}/buckets/{1}/translation-requests";
    private readonly string GetSubmissionsUrl = "/submission-service-api/v2/projects/{0}/buckets/{1}/translation-requests?limit={2}&offset={3}";
    private readonly string UpdateSubmissionUrl = "/submission-service-api/v2/projects/{0}/buckets/{1}/translation-requests/{2}";
    private readonly string GetRequestUrl = "/submission-service-api/v2/projects/{0}/buckets/{1}/translation-requests/{2}";

    private readonly string projectId;
    private readonly string bucketName;
    private readonly IAuthenticationStrategy auth;
    private const int PageSize = 500;

    public SubmissionApiClient(IAuthenticationStrategy auth, string projectId, string bucketName)
    {
      this.auth = auth;
      this.projectId = projectId;
      this.bucketName = bucketName;
    }

    public virtual TranslationRequest<TOriginalKey, TCustomRequest, TTargetKey, TCustomSubmission> CreateTranslationRequest(CreateTranslationRequest<TOriginalKey, TCustomRequest> submission)
    {
      var uriBuilder = this.GetRequestStringBuilder(string.Format(CreateSubmissionUrl, projectId, bucketName));
      var response = ExecutePostRequest(uriBuilder, submission, auth);
      return JsonConvert.DeserializeObject<TranslationRequest<TOriginalKey, TCustomRequest, TTargetKey, TCustomSubmission>>(response["response"]["data"].ToString());
    }

    public virtual TranslationRequest<TOriginalKey, TCustomRequest, TTargetKey, TCustomSubmission> CreateDetails(CreateSubmissionDetails<TTargetKey, TCustomSubmission> submission)
    {
      var uriBuilder = this.GetRequestStringBuilder(string.Format(CreateSubmissionUrl, projectId, bucketName));
      var response = ExecutePostRequest(uriBuilder, submission, auth);
      return JsonConvert.DeserializeObject<TranslationRequest<TOriginalKey, TCustomRequest, TTargetKey, TCustomSubmission>>(response["response"]["data"].ToString());
    }

    public virtual TranslationRequest<TOriginalKey, TCustomRequest, TTargetKey, TCustomSubmission> CreateSubmission(string translationRequestUid, List<CreateSubmissionRequest<TTargetKey, TCustomSubmission>> submissions)
    {
      var requestDetails = new CreateSubmissionDetails<TTargetKey, TCustomSubmission>();
      requestDetails.translationSubmissions = submissions;
      requestDetails.translationRequestUid = translationRequestUid;

      var uriBuilder = this.GetRequestStringBuilder(string.Format(CreateSubmissionUrl, projectId, bucketName));
      var response = ExecutePostRequest(uriBuilder, requestDetails, auth);
      return JsonConvert.DeserializeObject<TranslationRequest<TOriginalKey, TCustomRequest, TTargetKey, TCustomSubmission>>(response["response"]["data"].ToString());
    }

    public virtual TranslationRequest<TOriginalKey, TCustomRequest, TTargetKey, TCustomSubmission> UpdateTranslationRequest(UpdateTranslationRequest<TOriginalKey, TCustomRequest, TTargetKey, TCustomSubmission> request, string translationRequestUid)
    {
      var uriBuilder = this.GetRequestStringBuilder(string.Format(UpdateSubmissionUrl, projectId, bucketName, translationRequestUid));
      var response = ExecutePutRequest(uriBuilder, request, auth);
      return JsonConvert.DeserializeObject<TranslationRequest<TOriginalKey, TCustomRequest, TTargetKey, TCustomSubmission>>(response["response"]["data"].ToString());
    }

    public virtual TranslationRequest<TOriginalKey, TCustomRequest, TTargetKey, TCustomSubmission> Get(string translationRequestUid)
    {
      var uriBuilder = this.GetRequestStringBuilder(string.Format(GetRequestUrl, projectId, bucketName, translationRequestUid));
      var request = PrepareGetRequest(uriBuilder.ToString(), auth.GetToken());
      var response = ExecuteGetRequest(request, uriBuilder, auth);
      return JsonConvert.DeserializeObject<TranslationRequest<TOriginalKey, TCustomRequest, TTargetKey, TCustomSubmission>>(response["response"]["data"].ToString());
    }

    public virtual List<TranslationRequest<TOriginalKey, TCustomRequest, TTargetKey, TCustomSubmission>> GetAll(string searchField, string searchValue)
    {
      var page = GetPage(searchField, searchValue, PageSize, 0);
      var results = new List<TranslationRequest<TOriginalKey, TCustomRequest, TTargetKey, TCustomSubmission>>();
      var pageNumber = 0;
      results.AddRange(page.items);

      while (page.totalCount > results.Count && page.items.Count > 0)
      {
        pageNumber++;
        page = GetPage(null, PageSize, PageSize * pageNumber);
        results.AddRange(page.items);
      }

      return results;
    }

    public virtual SubmissionItemList<TOriginalKey, TCustomRequest, TTargetKey, TCustomSubmission> GetPage(string searchField, string searchValue, int limit, int offset)
    {
      var query = new Dictionary<string, string>();
      if (!string.IsNullOrEmpty(searchField))
      {
        query.Add(searchField, searchValue);
      }

      return GetPage(query, limit, offset);
    }

    public virtual SubmissionItemList<TOriginalKey, TCustomRequest, TTargetKey, TCustomSubmission> GetPage(Dictionary<string, string> query, int limit, int offset)
    {
      StringBuilder uriBuilder;
      uriBuilder = this.GetRequestStringBuilder(string.Format(GetSubmissionsUrl, projectId, bucketName, limit, offset));

      if(query != null && query.Keys.Count > 0)
      {
        BuildSearchQuery(query, uriBuilder);
      }
      
      var request = PrepareGetRequest(uriBuilder.ToString(), auth.GetToken());
      var response = ExecuteGetRequest(request, uriBuilder, auth);

      var settings = new JsonSerializerSettings
      {
        Error = (sender, args) =>
        {
          if (System.Diagnostics.Debugger.IsAttached)
          {
            System.Diagnostics.Debugger.Break();
          }
        }
      };

      return JsonConvert.DeserializeObject<SubmissionItemList<TOriginalKey, TCustomRequest, TTargetKey, TCustomSubmission>>(response["response"]["data"].ToString());
    }

    private static void BuildSearchQuery(Dictionary<string, string> query, StringBuilder uriBuilder)
    {
      uriBuilder.Append("&");
      var clauses = new List<string>();
      foreach (var key in query.Keys)
      {
        var fieldClauses = new List<string>();
        foreach (var val in query[key].Split('|'))
        {
          fieldClauses.Add(key + "=" + System.Net.WebUtility.UrlEncode(val));
        }

        clauses.Add(string.Join("&", fieldClauses.ToArray()));
      }

      uriBuilder.Append(string.Join("&", clauses.ToArray()));
    }
  }
}
