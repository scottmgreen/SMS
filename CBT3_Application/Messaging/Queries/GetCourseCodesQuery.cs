using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBT3_Application.Messaging.Queries
{
    public class GetCourseCodesQuery : BaseQueryBundle, IRequest<Result<List<String>>>
    {
        public bool SIDAOnly { get; set; }
        public Trainee Trainee { get; set; }
        public GetCourseCodesQuery(Trainee trainee,bool sidaOnly)
        {
            Trainee = trainee;
            SIDAOnly = sidaOnly;
        }


    }
}
