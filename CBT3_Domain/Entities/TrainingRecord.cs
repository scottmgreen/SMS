using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml;

namespace CBT3_Domain.Entities;
public class TrainingRecord : IRecord
{


    #region Data

    //[TrainingID]           INT            IDENTITY (1, 1) NOT NULL,
    //[UPID]                 INT            NOT NULL,
    //[DOBYear]              INT            NOT NULL,
    //[FirstName]            VARCHAR (30)   NOT NULL,
    //[LastName]             VARCHAR (30)   NOT NULL,
    //[CourseStartDate]      DATETIME       NOT NULL,
    //[CourseEndDate]        DATETIME       NOT NULL,
    //[CourseExpirationDate] DATETIME       NOT NULL,
    //[CourseCode]           INT            NOT NULL,
    //[CoursePassed]         BIT            NOT NULL,
    //[Score]                NUMERIC (3, 2) NOT NULL,
    //[WorkstationID]        VARCHAR (15)   NOT NULL,
    //[Comments]             VARCHAR (100)  NULL,
    //[RecordLastUpdated]    DATETIME       NOT NULL,
    //[RecordLastUpdateUser] VARCHAR (30)   NOT NULL,
    //[RowVersion]           ROWVERSION     NOT NULL,


    #endregion Data



    #region Private Attributes

    /// <summary>
    /// private backing field
    /// </summary>
    private int _TrainingID;
    private int _UPID;
    private int _DYOB = -1;
    private string _FirstName;
    private string _LastName;
    private bool _SubscribeToEmailNewsletter;
    private bool _SubscribeToTextNewsletter;
    private bool _SubscribeToOperationalTexts;

    private DateTime _CourseStartDate;
    private DateTime _CourseEndDate;
    private DateTime _CourseExpirationDate;
    private string _CourseCode;
    private string _CourseDescription;
    private bool _CoursePassed;
    private int _Score;
    private string _WorkstationID;
    private string _Comments = string.Empty;
    private DateTime _RecordLastUpdated;
    private string _RecordLastUpdateUser;
    private byte[] _RowVersion;
    private string _RecordXML;


    /// <summary>
    /// Const used to identify BO Property Name
    /// </summary>
    //private const string cn_pnTrainingID = "TrainingID";
    //private const string cn_pnUPID = "UPID";
    //private const string cn_pnDOBYear = "DYOB";
    //private const string cn_pnFirstName = "First Name";
    //private const string cn_pnLastName = "LastName";
    //private const string cn_pnCourseStartDate = "CourseStartDate";
    //private const string cn_pnCourseEndDate = "CourseEndDate";
    //private const string cn_pnCourseExpirationDate = "CourseExpirationDate";
    //private const string cn_pnCourseCode = "CourseCode";
    //private const string cn_pnCourseDescription = "CourseDescription";
    //private const string cn_pnCoursePassed = "CoursePassed";
    //private const string cn_pnScore = "Score";
    //private const string cn_pnWorkstationID = "WorkstationID";
    //private const string cn_pnComments = "Comments";
    //private const string cn_pnRecordLastUpdated = "RecordLastUpdated";
    //private const string cn_pnRecordLastUpdateUser = "RecordLastUpdateUser";
    //private const string cn_pnRowVersion = "RowVersion";
    //private const string cn_pnRecordXML = "RecordXML";


    #endregion Private Attributes

    #region Properties

    //TrainingId

    /// <summary>
    /// BO Property
    /// </summary>

    [Required()]
    public int TrainingID
    {
        get => _TrainingID;
        set
        {
            if (_TrainingID != value)
            {
                _TrainingID = value;

            }
        }
    }

    //UPID

    /// <summary>
    /// BO Property
    /// </summary>

    [Required]
    public int UPID
    {
        get => _UPID;
        set
        {
            if (_UPID != value)
            {
                _UPID = value;

            }
        }
    }

    //DOBYear

    /// <summary>
    /// BO Property
    /// </summary>

    [Required]
    public int DYOB
    {
        get => _DYOB;
        set
        {
            if (_DYOB != value)
            {

                _DYOB = value;

            }
        }
    }

    //FirstName

    /// <summary>
    /// BO Property
    /// </summary>

    [Required]
    public string FirstName
    {
        get => _FirstName;
        set
        {
            if (_FirstName != value)
            {

                _FirstName = value;

            }
        }
    }

    //LastName

    /// <summary>
    /// BO Property
    /// </summary>

    [Required]
    public string LastName
    {
        get => _LastName;
        set
        {
            if (_LastName != value)
            {

                _LastName = value;

            }
        }
    }

    [Required]
    public bool SubscribeToEmailNewsletter
    {
        get => _SubscribeToEmailNewsletter;
        set
        {
            if (_SubscribeToEmailNewsletter != value)
            {

                _SubscribeToEmailNewsletter = value;

            }
        }
    }

    [Required]
    public bool SubscribeToTextNewsletter
    {
        get => _SubscribeToTextNewsletter;
        set
        {
            if (_SubscribeToTextNewsletter != value)
            {

                _SubscribeToTextNewsletter = value;

            }
        }
    }
    [Required]
    public bool SubscribeToOperationalTexts
    {
        get => _SubscribeToOperationalTexts;
        set
        {
            if (_SubscribeToOperationalTexts != value)
            {

                _SubscribeToOperationalTexts = value;

            }
        }
    }
    //CourseStartDate

    /// <summary>
    /// BO Property
    /// </summary>

    [DisplayFormat(DataFormatString = "{0:MM-dd-yyyy H:mm}", ApplyFormatInEditMode = true)]
    [Required]
    public DateTime CourseStartDate
    {
        get => _CourseStartDate;
        set
        {
            if (_CourseStartDate != value)
            {
                _CourseStartDate = value;

            }
        }
    }

    //CourseEndDate

    /// <summary>
    /// BO Property
    /// </summary>

    [DisplayFormat(DataFormatString = "{0:H:mm}", ApplyFormatInEditMode = true)]
    [Required]
    public DateTime CourseEndDate
    {
        get => _CourseEndDate;
        set
        {
            if (_CourseEndDate != value)
            {
                _CourseEndDate = value;

            }
        }
    }

    //CourseExpirationDate
    /// <summary>
    /// BO Property
    /// </summary>
    [DisplayName("Expiration Date")]
    [DisplayFormat(DataFormatString = "{0:MM-dd-yyyy}", ApplyFormatInEditMode = true)]
    [Required]
    public DateTime CourseExpirationDate
    {
        get => _CourseExpirationDate;
        set
        {
            if (_CourseExpirationDate != value)
            {
                _CourseExpirationDate = value;

            }
        }
    }

    //CourseCode

    /// <summary>
    /// BO Property
    /// </summary>
    [DisplayName("Course Code")]
    [Required]
    public string CourseCode
    {
        get => _CourseCode;
        set
        {
            if (_CourseCode != value)
            {
                _CourseCode = value;

            }
        }
    }

    //Course Description

    /// <summary>
    /// BO Property
    /// </summary>
    [DisplayName("Course Description")]
    [Required]
    public string CourseDescription
    {
        get => $"{_CourseCode}-{_CourseDescription}";
        set
        {
            if (_CourseDescription != value)
            {
                _CourseDescription = value;

            }
        }
    }

    //CoursePassed

    /// <summary>
    /// BO Property
    /// </summary>
    [DisplayName("Passed")]
    public bool CoursePassed
    {
        get => _CoursePassed;
        set
        {
            if (_CoursePassed != value)
            {
                _CoursePassed = value;

            }
        }
    }

    //Score

    /// <summary>
    /// BO Property
    /// </summary>
    [DisplayName("Score")]
    [Required]
    public int Score
    {
        get => _Score;
        set
        {
            if (_Score != value)
            {
                _Score = value;

            }
        }
    }

    //WorkstationID

    /// <summary>
    /// BO Property
    /// </summary>
    [DisplayName("Training Station")]
    public string WorkstationID
    {
        get => _WorkstationID;
        set
        {
            if (_WorkstationID != value)
            {
                _WorkstationID = value;

            }
        }
    }

    //Comments

    /// <summary>
    /// BO Property
    /// </summary>
    [DisplayName("Comments")]
    [Required]
    public string Comments
    {
        get => _Comments ?? "Test Comment";
        set
        {
            if (_Comments != value)
            {
                _Comments = value;

            }
        }
    }

    //RecordLastUpdated

    /// <summary>
    /// BO Property
    /// </summary>
    [DisplayName("Last Updated")]
    public DateTime RecordLastUpdated
    {
        get => _RecordLastUpdated;
        set
        {
            if (_RecordLastUpdated != value)
            {
                _RecordLastUpdated = value;

            }
        }
    }

    //RecordLastUpdateUser

    /// <summary>
    /// BO Property
    /// </summary>
    [DisplayName("User")]
    public string RecordLastUpdateUser
    {
        get => _RecordLastUpdateUser;
        set
        {
            if (_RecordLastUpdateUser != value)
            {
                _RecordLastUpdateUser = value;

            }
        }
    }

    //RowVersion

    /// <summary>
    /// BO Property
    /// </summary>
    [Timestamp]
    [ConcurrencyCheck()]
    public byte[] RowVersion
    {
        get => _RowVersion;
        set
        {
            if (_RowVersion != value)
            {
                _RowVersion = value;

            }
        }
    }

    /// <summary>
    /// BO Property
    /// </summary>
    public string RecordXML
    {
        get => _RecordXML;
        set
        {
            if (_RecordXML != value)
            {
                _RecordXML = value;
            }
        }
    }


    #endregion Properties

    #region Constructors

        
    /// <summary>
    /// Private method used to populate new BO
    /// </summary>


    #endregion Constructors

    #region Methods

   #endregion Methods
}

