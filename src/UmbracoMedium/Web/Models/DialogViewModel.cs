using System.Collections.Generic;

/// <summary>
/// Summary description for DialogViewModel
/// </summary>
public class DialogViewModel
{
    public string ErrorMessage { get; set; }
    public string Name { get; set; }
    public string Username { get; set; }
    public  string UserImageUrl { get; set; }
    public string UserUrl { get; set; }
    public IDictionary<string,string> Publications { get; set; }
    public string Status { get; set; } // no_token, error, ok
    public string AuthUrl { get; set; }
    public string MediumPostUrl { get; set; }
}