public class GoogleBookAPIResponse
{
    public string Kind { get; set; }
    public long TotalItems { get; set; }
    public List<GoogleBookResponse> Items { get; set; } = new List<GoogleBookResponse>();
}

public class GoogleBookResponse
{
    public string kind { get; set; }
    public string id { get; set; }
    public string eTag { get; set; }
    public string selfLink { get; set; }
    public VolumeInfo VolumeInfo { get; set; }
}

public class VolumeInfo
{
    public string Title { get; set; }
    public string Subtitle { get; set; }
    public List<string> Authors { get; set; }
    public string Publisher { get; set; }
    public string PublishedDate { get; set; }
    public string Description { get; set; }
    public List<IndustryIdentifier> IndustryIdentifiers { get; set; } = new List<IndustryIdentifier>();
    public ImageLinks ImageLinks { get; set; }
    public double AverageRating { get; set; }
    public int RatingsCount { get; set; }
}

public class ImageLinks
{
    public string SmallThumbnail { get; set; }
    public string Thumbnail { get; set; }
    public string Small { get; set; }
    public string Medium { get; set; }
    public string Large { get; set; }
    public string ExtraLarge { get; set; }
}

public class IndustryIdentifier
{
    public string Type { get; set; }
    public string Identifier { get; set; }
}
