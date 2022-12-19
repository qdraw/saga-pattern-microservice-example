using System.Collections.Generic;

namespace ACME.Library.Common.Models.Api
{
    public class ApiResponse
    {
        public string ErrorCode { get; set; }
        public string Message { get; set; }
        public bool IsSuccess { get; set; }
        public List<string> ValidationErrors { get; set; }

        public void SetErrorCode(string errorCode)
        {
            IsSuccess = false;
            ErrorCode = errorCode;
        }
        public static ApiResponse Success()
        {
            return new ApiResponse { IsSuccess = true };
        }
        public static ApiResponse Fail(string errorCode)
        {
            return new ApiResponse { IsSuccess = false, ErrorCode = errorCode };
        }
        public static ApiResponse Fail(string errorCode, string message)
        {
            return new ApiResponse { IsSuccess = false, ErrorCode = errorCode, Message = message };
        }
        public static ApiResponse FailValidation(List<string> errors)
        {
            return new ApiResponse
            {
                IsSuccess = false,
                ValidationErrors = errors
            };
        }
    }

    public class ApiResponse<T> : ApiResponse
    {
        public ApiResponse(T data = default)
        {
            Data = data;
        }
        
        public T Data { get; set; }

        public static ApiResponse<T> Success(T data)
        {
            return new ApiResponse<T> { IsSuccess = true, Data = data };
        }
    }
}