﻿using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Smartling.Api.Authentication;
using Smartling.Api.Exceptions;
using Smartling.Api.Model;

namespace Smartling.Api.Batch
{
  public class BatchApiClient : ApiClientBase
  {
    private readonly string CreateBatchUrl = "/jobs-batch-api/v1/projects/{0}/batches";
    private readonly string UploadBatchUrl = "/jobs-batch-api/v1/projects/{0}/batches/{1}/file";
    private readonly string ExecuteBatchUrl = "/jobs-batch-api/v1/projects/{0}/batches/{1}";
    private readonly string GetBatchUrl = "/jobs-batch-api/v1/projects/{0}/batches/{1}";
    
    private const string FileUriParameterName = "fileUri";
    private const string FileTypeParameterName = "fileType";
    private const string NameSpaceParameterName = "smartling.namespace";
    private const string CallbackUrlParameterName = "callbackUrl";
    private const string LocalesToApproveParameterName = "localeIdsToAuthorize[]";
    private const string CliendUidParameterName = "smartling.client_lib_id";
    private const string LocalesSeparator = ",";

    private readonly string projectId;
    private readonly string callbackUrl;
    private readonly IAuthenticationStrategy auth;

    public BatchApiClient(IAuthenticationStrategy auth, string projectId, string callbackUrl)
    {
      this.auth = auth;
      this.projectId = projectId;
      this.callbackUrl = callbackUrl;
    }

    public virtual Model.Batch Create(CreateBatch batch)
    {
      var uriBuilder = this.GetRequestStringBuilder(string.Format(CreateBatchUrl, projectId));
      var response = ExecutePostRequest(uriBuilder, batch, auth);
      return JsonConvert.DeserializeObject<Model.Batch>(response["response"]["data"].ToString());
    }

    public virtual BatchUploadResult UploadFile(BatchUpload batch)
    {
      using (var fileStream = new FileStream(batch.FilePath, FileMode.Open, FileAccess.Read))
      {
        return UploadFileStream(batch, fileStream);
      }
    }

    public virtual BatchUploadResult UploadFileStream(BatchUpload batch, FileStream fileStream)
    {
      var uriBuilder = this.GetRequestStringBuilder(string.Format(UploadBatchUrl, projectId, batch.BatchUid));
      var formData = new NameValueCollection();
      formData.Add(FileUriParameterName, batch.FileUri);
      formData.Add(FileTypeParameterName, batch.FileType);
      formData.Add(CliendUidParameterName, JsonConvert.SerializeObject(this.ApiClientUid));
      formData.Add(LocalesToApproveParameterName, string.Join(LocalesSeparator, batch.ApprovedLocales));

      if (!string.IsNullOrEmpty(this.callbackUrl))
      {
        formData.Add(CallbackUrlParameterName, this.callbackUrl);
      }

      if (!string.IsNullOrEmpty(batch.NameSpace))
      {
        formData.Add(NameSpaceParameterName, batch.NameSpace);
      }

      return ExecuteUploadRequest(fileStream, batch.FileUri, uriBuilder, formData);
    }

    private BatchUploadResult ExecuteUploadRequest(Stream fileStream, string fileUri, StringBuilder uriBuilder, NameValueCollection formData)
    {
      try
      {
        var request = PrepareFilePostRequest(uriBuilder.ToString(), fileUri, fileStream, formData, auth.GetToken());
        var response = JObject.Parse(GetResponse(request));
        return JsonConvert.DeserializeObject<BatchUploadResult>(response["response"]["data"].ToString());
      }
      catch (AuthenticationException)
      {
        var request = PrepareFilePostRequest(uriBuilder.ToString(), fileUri, fileStream, formData, auth.GetToken(true));
        var response = JObject.Parse(GetResponse(request));
        return JsonConvert.DeserializeObject<BatchUploadResult>(response["response"]["data"].ToString());
      }
    }
    
    public virtual void Execute(string batchUid)
    {
      var uriBuilder = this.GetRequestStringBuilder(string.Format(ExecuteBatchUrl, projectId, batchUid));
      ExecutePostRequest(uriBuilder, new ExecuteBatch() {action = "EXECUTE"}, auth);
    }

    public virtual BatchResult Get(string batchUid)
    {
      var uriBuilder = this.GetRequestStringBuilder(string.Format(GetBatchUrl, projectId, batchUid));

      var request = PrepareGetRequest(uriBuilder.ToString(), auth.GetToken());
      var response = ExecuteGetRequest(request, uriBuilder, auth);
      return JsonConvert.DeserializeObject<BatchResult>(response["response"]["data"].ToString());
    }
  }
}
