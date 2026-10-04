using Microsoft.AspNetCore.Mvc;
using MusicAlbumMicroservice.Application.Common;

namespace MusicAlbumMicroservice.Api;

internal static class ResultResponses
{
    public static ActionResult ToProblem(this ControllerBase controller, Error error)
    {
        if (error.Code == ErrorCode.InvalidInput)
        {
            controller.ModelState.AddModelError("request", error.Message);
            return controller.ValidationProblem(modelStateDictionary: controller.ModelState);
        }

        return controller.Problem(statusCode: StatusCodes.Status404NotFound,
            title: error.Code.ToString(), detail: error.Message);
    }
}