﻿namespace RoliSoft.TVShowTracker.Parsers.Downloads.Engines.Torrent
{
    using System;
    using System.Collections.Generic;
    using System.Security.Authentication;

    using NUnit.Framework;

    /// <summary>
    /// Provides support for scraping FileList.ro.
    /// </summary>
    [Parser("RoliSoft", "2011-08-16 16:12 PM"), TestFixture]
    public class FileList : DownloadSearchEngine
    {
        /// <summary>
        /// Gets the name of the site.
        /// </summary>
        /// <value>The name.</value>
        public override string Name
        {
            get
            {
                return "FileList";
            }
        }

        /// <summary>
        /// Gets the URL of the site.
        /// </summary>
        /// <value>The site location.</value>
        public override string Site
        {
            get
            {
                return "http://filelist.ro/";
            }
        }

        /// <summary>
        /// Gets a value indicating whether the site requires authentication.
        /// </summary>
        /// <value><c>true</c> if requires authentication; otherwise, <c>false</c>.</value>
        public override bool Private
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the names of the required cookies for the authentication.
        /// </summary>
        /// <value>The required cookies for authentication.</value>
        public override string[] RequiredCookies
        {
            get
            {
                return new[] { "uid", "pass" };
            }
        }

        /// <summary>
        /// Gets a value indicating whether this search engine can login using a username and password.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this search engine can login; otherwise, <c>false</c>.
        /// </value>
        public override bool CanLogin
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets the URL to the login page.
        /// </summary>
        /// <value>The URL to the login page.</value>
        public override string LoginURL
        {
            get
            {
                return Site + "takelogin.php";
            }
        }

        /// <summary>
        /// Gets the input fields of the login form.
        /// </summary>
        /// <value>The input fields of the login form.</value>
        public override Dictionary<string, object> LoginFields
        {
            get
            {
                return new Dictionary<string, object>
                    {
                        { "username", LoginFieldTypes.UserName },
                        { "password", LoginFieldTypes.Password },
                    };
            }
        }

        /// <summary>
        /// Gets the type of the link.
        /// </summary>
        /// <value>The type of the link.</value>
        public override Types Type
        {
            get
            {
                return Types.Torrent;
            }
        }

        /// <summary>
        /// Searches for download links on the service.
        /// </summary>
        /// <param name="query">The name of the release to search for.</param>
        /// <returns>List of found download links.</returns>
        public override IEnumerable<Link> Search(string query)
        {
            var html = Utils.GetHTML(Site + "browse.php?searchin=0&sort=0&search=" + Uri.EscapeUriString(query), cookies: Cookies);

            if (GazelleTrackerLoginRequired(html.DocumentNode))
            {
                throw new InvalidCredentialException();
            }

            var links = html.DocumentNode.SelectNodes("//table/tr/td[2]/a/b");
            
            if (links == null)
            {
                yield break;
            }

            foreach (var node in links)
            {
                var link = new Link(this);

                link.Release = node.GetNodeAttributeValue("../", "title") ?? node.InnerText;
                link.InfoURL = Site + node.GetNodeAttributeValue("../../a", "href");
                link.FileURL = Site + node.GetNodeAttributeValue("../../../td[3]/a", "href");
                link.Size    = node.GetHtmlValue("../../../td[7]").Replace("<br>", " ");
                link.Quality = FileNames.Parser.ParseQuality(link.Release);
                link.Infos   = Link.SeedLeechFormat.FormatWith(node.GetTextValue("../../../td[9]").Trim(), node.GetTextValue("../../../td[10]").Trim())
                             + (node.GetTextValue("../../b/small/font") == "[FreeLeech]" ? ", Free" : string.Empty);
                
                yield return link;
            }
        }

        /// <summary>
        /// Authenticates with the site and returns the cookies.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>Cookies on success, <c>string.Empty</c> on failure.</returns>
        public override string Login(string username, string password)
        {
            return GazelleTrackerLogin(username, password);
        }
    }
}
