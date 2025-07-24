using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CBT3_Application.Messaging.Queries;

namespace CBT3_Application.Messaging.QueryHandlers
{
    public class GetCourseCodesQueryHandler : BaseQueryBundle, IRequestHandler<GetCourseCodesQuery, Result<List<string>>>
    {
        private TrainingService _trainingService;
        public GetCourseCodesQueryHandler(TrainingService trainingService)
        {
            _trainingService = trainingService;
        }
        public Task<Result<List<string>>> HandleAsync(GetCourseCodesQuery request, CancellationToken ct = default)
        {
            Result<List<string>> list = Task.Run(() => _trainingService.GetCourseCodesAsync(request.Trainee,request.SIDAOnly,ct)).Result;
            return Task.FromResult(list);
        }
    }
}
