using System.Data;

namespace SMS_Shared.Common;

public static class LinqExtensions
{
    
    //private static string _globalcoursespath = "";//CBT_Configs.Default.CoursesPath;


    //public static string GetAbsolutePath(this string basePath, string path)
    //{
    //    if (path.Contains("AUD_"))
    //    { basePath = basePath + "\\audio\\"; }
    //    if (path.Contains("VID_"))
    //    { basePath = basePath + "\\video\\"; }
    //    if (path.Contains("IMG_"))
    //    { basePath = basePath + "\\images\\"; }

    //    if (string.IsNullOrEmpty(path))
    //    {
    //        return string.Empty;
    //    }

    //    switch (path)
    //    {
    //        case string a when a.Contains("../_"):
    //            basePath = _globalcoursespath;
    //            path = path.Replace("../", "");
    //            break;
    //        //case string a when a.Contains("assets"):
    //        //    basePath = _globalbasepath;
    //        //    path = path.Replace("../../", "");
    //        //    break;
    //        default:
    //            break;
    //    }




    //    string finalPath;
    //    // specific for windows paths starting on \ - they need the drive added to them.

    //    if (!Path.IsPathRooted(path) || "\\".Equals(Path.GetPathRoot(path)))
    //    {
    //        if (path.StartsWith(Path.DirectorySeparatorChar.ToString()))
    //            finalPath = Path.Combine(Path.GetPathRoot(basePath), path.TrimStart(Path.DirectorySeparatorChar));
    //        else
    //            finalPath = Path.Combine(basePath, path);
    //    }
    //    else
    //        finalPath = path;
    //    // resolves any internal "..\" to get the true full path.
    //    //Final check ... FILE EXISTS
    //    string testresults = Path.GetFullPath(finalPath);
    //    string results = string.Empty;
    //    if (File.Exists(testresults))
    //    {
    //        results = testresults;
    //    }

    //    return results;
    //}
    //public static string GetAbsolutePath(this string path)
    //{
    //    return GetAbsolutePath(null, path);
    //}




    //This method is to handle if element is missing
    public static string ElementValueNull(this XElement element)
    {
        if (element != null)
            return element.Value;

        return "";
    }

    //This method is to handle if attribute is missing
    public static string AttributeValueNull(this XElement element, string attributeName)
    {
        if (element is null)
            return "";
        else
        {
            XAttribute attr = element.Attribute(attributeName);
            return attr is null ? "" : attr.Value;
        }
    }


    public static T GetNext<T>(this IEnumerable<T> list, T current)
    {
        try
        {
            return list.SkipWhile(x => !x.Equals(current)).Skip(1).First();
        }
        catch
        {
            return default;
        }
    }

    public static T GetPrevious<T>(this IEnumerable<T> list, T current)
    {
        try
        {
            return list.TakeWhile(x => !x.Equals(current)).Last();
        }
        catch
        {
            return default;
        }
    }


    //public static IEnumerable<Control> AsEnumerable
    //(this Control.ControlCollection @this)
    //{
    //    foreach (object control in @this)
    //        yield return (Control)control;
    //}

    //public static IEnumerable<T> GetAllControls<T>(this Control @this) where T : Control
    //{
    //    return @this.Controls.AsEnumerable().Where(x => x.GetType() == typeof(T)).
    //    Select(y => (T)y).
    //        Union(@this.Controls.AsEnumerable().
    //        SelectMany(x => GetAllControls<T>(x)).
    //        Select(y => (T)y));
    //}

    public static void ForEach<T>(this IEnumerable<T> @this, Action<T> action)
    {
        foreach (T t in @this)
        {
            action(t);
        }
    }


    //Thread 
    public static void OnThread(this object @object, Action action)
    {
        ThreadPool.QueueUserWorkItem(state =>
        {
            action();
        });
    }

    public static void OnThread<T>(this object @object, Action<T> action, T argument)
    {
        ThreadPool.QueueUserWorkItem(state =>
        {
            action(argument);
        });
    }


    public static IEnumerable<long> DistributeLong(this long total, int divider)
    {
        if (divider == 0)
        {
            yield return 0;
        }
        else
        {
            long rest = total % divider;
            double result = total / (double)divider;

            for (long i = 0; i < divider; i++)
            {
                if (rest-- > 0)
                    yield return (long)Math.Ceiling(result);
                else
                    yield return (long)Math.Floor(result);
            }
        }
    }


    public static int Round(this int i, int nearest)
    {
        if (nearest <= 0 || nearest % 10 != 0)
            throw new ArgumentOutOfRangeException("nearest", "Must round to a positive multiple of 10");

        return (i + 5 * nearest / 10) / nearest * nearest;
    }


}
