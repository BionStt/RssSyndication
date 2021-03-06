﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using WilderMinds.RssSyndication;
using Xunit;

namespace RssSyndication.Tests
{
  public class FeedFacts
  {

    Feed CreateTestFeed()
    {
      var feed = new Feed
      {
        Title = "Shawn Wildermuth's Blog",
        Description = "My Favorite Rants and Raves",
        Link = new Uri("http://wildermuth.com/feed"),
        Copyright = "(c) 2016"
      };

      var item1 = new Item
      {
        Title = "Foo Bar",
        Body = "<p>Foo bar</p>",
        Link = new Uri("http://foobar.com/item#1"),
        Permalink = "http://foobar.com/item#1",
        PublishDate = DateTime.UtcNow,
        Author = new Author { Name = "Shawn Wildermuth", Email = "shawn@wildermuth.com" }
      };

      item1.Categories.Add("aspnet");
      item1.Categories.Add("foobar");

      item1.Comments = new Uri("http://foobar.com/item1#comments");

      feed.Items.Add(item1);

      var item2 = new Item
      {
        Title = "Quux",
        Body = "<p>Quux</p>",
        Link = new Uri("http://quux.com/item#1"),
        Permalink = "http://quux.com/item#1",
        PublishDate = DateTime.UtcNow,
        Author = new Author { Name = "Shawn Wildermuth", Email = "shawn@wildermuth.com" }
      };

      item1.Categories.Add("aspnet");
      item1.Categories.Add("quux");

      feed.Items.Add(item2);

      return feed;
    }

    [Fact]
    public void CreatesValidRss()
    {
      var feed = CreateTestFeed();

      var rss = feed.Serialize();
      Debug.Write(rss);
      var doc = XDocument.Parse(rss);

      Assert.NotNull(doc);
      var item = doc.Descendants("item").FirstOrDefault();
      Assert.NotNull(item);
      Assert.True(item.Element("title").Value == "Foo Bar", "First Item was correct");
    }

    [Fact]
    public void DatesAreProperlyFormatted()
    {
      CultureInfo.CurrentCulture = new CultureInfo("ru-RU");
      var feed = CreateTestFeed();
      var rss = feed.Serialize();
      var doc = XDocument.Parse(rss);
      var pubDate = doc.Descendants("pubDate").First();

      var rfc822FormattedDate = feed.Items.First().PublishDate.ToString("r", CultureInfo.InvariantCulture);
      Assert.Equal(rfc822FormattedDate, pubDate.Value);
    }

    [Fact]
    public void FeedAddsItems()
    {
      var feed = CreateTestFeed();

      Assert.NotNull(feed.Items.First());
      Assert.True(feed.Items.First().Title == "Foo Bar");
      Assert.True(feed.Items.ElementAt(1).Title == "Quux");
      Assert.True(feed.Items.First().Author.Name == "Shawn Wildermuth");
    }

    [Fact]
    public void FeedIsCreated()
    {
      var feed = new Feed
      {
        Title = "Shawn Wildermuth's Blog",
        Description = "My Favorite Rants and Raves",
        Link = new Uri("http://wildermuth.com/feed"),
        Copyright = "(c) 2016"
      };

      Assert.NotNull(feed);
      Assert.True(feed.Title == "Shawn Wildermuth's Blog");
      Assert.True(feed.Description == "My Favorite Rants and Raves");
      Assert.True(feed.Link == new Uri("http://wildermuth.com/feed"));
      Assert.True(feed.Copyright == "(c) 2016");
    }

    [Fact]
    public void AtomIsSupported()
    {
      var feed = new Feed
      {
        Title = "Shawn Wildermuth's Blog",
        Description = "My Favorite Rants and Raves",
        Link = new Uri("http://wildermuth.com/feed")
      };

      Assert.NotNull(feed);
      Assert.Null(feed.Copyright);
    }


    [Fact]
    public void CopyrightIsOptional()
    {
      var feed = CreateTestFeed();

      Assert.NotNull(feed);
      var rss = feed.Serialize();
      Assert.Contains("http://www.w3.org/2005/Atom", rss);
    }

    [Fact]
    public void GeneratedXmlContainsDeclaration()
    {
      var feed = CreateTestFeed();
      var rss = feed.Serialize();
      Assert.StartsWith("<?xml version", rss);
    }

    [Fact]
    public void GeneratedXmlHonorsSerializeOption()
    {
      var feed = CreateTestFeed();

      var defaultRss = feed.Serialize();
      var withOption = feed.Serialize(new SerializeOption() { Encoding = Encoding.UTF8 });

      // verify encoding
      Assert.Contains("utf-16", new StringReader(defaultRss).ReadLine());
      Assert.Contains("utf-8", new StringReader(withOption).ReadLine());
    }
  }

}