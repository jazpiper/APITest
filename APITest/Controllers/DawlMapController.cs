using ApiHelpers;
using ApiLogManager;
using ApiModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace APITest.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DawlMapController : ControllerBase
    {

        [HttpPost]
        public async Task<ActionResult<DistanceResponse>> Distances(DistanceRequest request)
        {
            var httpContext = Request.HttpContext;

            var apiCode = httpContext?.Request.Headers["API_COM_GBN"];
            var apiTraceNo = httpContext?.Request.Headers["API_TRACE_NO"];

            try
            {
                request.API_COM_GBN = apiCode;
                request.API_TRACE_NO = apiTraceNo;

                ApiLogger.Debug($"[{request.API_TRACE_NO}] DISTANCES REQUEST : {JsonSerializer.Serialize(request)}");

                if (request == null)
                    return new DistanceResponse { LINEAR_DISTANCE = 0, NAVIGATION_DISTANCE = 0 };

                // 직선거리 구하기

                //ApiLogger.Debug($"[{request.API_TRACE_NO}] 1. Before LinearDistance Request");

                var linearDistance = DistanceHelper.LinearDistance(request);

                //ApiLogger.Debug($"[{request.API_TRACE_NO}] 2. After LinearDistance Request");

                //ApiLogger.Debug($"[{request.API_TRACE_NO}] 3. Before NavigationDistance Request");

                var navigationDistance = 0d;

                // 연동사 가맹점코드가 있고, 해당 가맹점 또는 지점의 거리 계산 구분이 내비거리 일 때만 내비거리 계산

                if (!string.IsNullOrEmpty(request.SHOP_CODE))
                {
                    // 연동사 가맹점 코드가 없으면 내비거리 계산

                    // 가맹점의 거리 계산 구분 가져오기
                    var isNeccesaryCalculateNavigationDistance =
                        await DistanceHelper.IsNecessaryNavigationDistance(request.API_COM_GBN, request.SHOP_CODE, request.API_TRACE_NO);

                    if (isNeccesaryCalculateNavigationDistance)
                    {
                        // 내비거리 계산
                        ApiLogger.Debug($"[{request.API_TRACE_NO}] SHOP_CODE : [{request.SHOP_CODE}], NAVIGATION DISTANCE CALCULATE.");
                        navigationDistance = await DistanceHelper.NavigationDistanceAsync(request);
                    }
                    else
                    {
                        // 내비거리 계산 하지 않음. 1로 고정해서 Response
                        ApiLogger.Debug($"[{request.API_TRACE_NO}] SHOP_CODE : [{request.SHOP_CODE}], NAVIGATION DISTANCE CALCULATE IGNORED.");
                        navigationDistance = 1;
                    }
                }
                else
                {
                    ApiLogger.Debug($"[{request.API_TRACE_NO}] SHOP_CODE : [{request.SHOP_CODE}], SHOP_CODE IS NOT VALID, NAVIGATION DISTANCE 'FORCE' CALCULATE.");

                    navigationDistance = await DistanceHelper.NavigationDistanceAsync(request);

                    ApiLogger.Debug($"[{request.API_TRACE_NO}] SHOP_CODE : [{request.SHOP_CODE}], NAVIGATION DISTANCE : [{navigationDistance}]");
                }

                //ApiLogger.Debug($"[{request.API_TRACE_NO}] 4. After NavigationDistance Request");

                var response = new DistanceResponse
                {
                    LINEAR_DISTANCE = linearDistance,
                    NAVIGATION_DISTANCE = navigationDistance
                };

                ApiLogger.Debug($"[{request.API_TRACE_NO}] DISTANCES RESPONSE : {JsonSerializer.Serialize(response)}");

                return new DistanceResponse
                {
                    LINEAR_DISTANCE = linearDistance,
                    NAVIGATION_DISTANCE = navigationDistance
                };
            }
            catch (Exception e)
            {
                ApiLogger.Warn(e);
                return new DistanceResponse
                {
                    LINEAR_DISTANCE = 0,
                    NAVIGATION_DISTANCE = 0
                };
            }
        }
    }
}
