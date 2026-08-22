namespace ScrapWebsite.ViewModels;

public class SharedSeoViewModel
{
    public string Title { get; set; } = "ScrapWebsite";

    public string Description { get; set; } = "Website thu mua phe lieu render bang ASP.NET Core MVC.";

    public string? Keywords { get; set; }

    public string? CanonicalUrl { get; set; }

    public string? OgTitle { get; set; }

    public string? OgDescription { get; set; }

    public string? OgImage { get; set; }

    public string OgType { get; set; } = "website";

    public bool RobotsIndex { get; set; } = true;

    public bool RobotsFollow { get; set; } = true;
}
