using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBT3_Domain.Interfaces;
public interface IRecord
{
    /// <summary>
    /// 
    /// </summary>
    int UPID { get; set; }
    /// <summary>
    /// 
    /// </summary>
    int DYOB { get; set; }
    /// <summary>
    /// 
    /// </summary>
    string FirstName { get; set; }
    /// <summary>
    /// 
    /// </summary>
    string LastName { get; set; }
    /// <summary>
    /// 
    /// </summary>
    string CourseDescription { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public DateTime CourseStartDate { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public DateTime CourseEndDate { get; set; }
    ///

}
