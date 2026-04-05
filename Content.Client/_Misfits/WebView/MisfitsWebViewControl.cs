// #Misfits Add - Domain-locked WebView wrapper for Misfits wiki integration
using System;
using Robust.Client.UserInterface;
using Robust.Client.WebView;

namespace Content.Client._Misfits.WebView;

/// <summary>
/// Wraps <see cref="WebViewControl"/> and locks all navigation to ss14.misfitsystems.net.
/// No address bar or browser chrome is exposed to players.
/// </summary>
public sealed class MisfitsWebViewControl : Control
{
    private const string AllowedHost = "ss14.misfitsystems.net";

    private readonly WebViewControl _wv;

    public MisfitsWebViewControl()
    {
        HorizontalExpand = true;
        VerticalExpand = true;

        _wv = new WebViewControl
        {
            HorizontalExpand = true,
            VerticalExpand = true
        };

        // Cancel all navigation to hosts outside our allowed domain
        _wv.AddBeforeBrowseHandler(OnBeforeBrowse);

        AddChild(_wv);
    }

    /// <summary>Navigate to a URL. Off-domain URLs are silently ignored.</summary>
    public void NavigateTo(string url)
    {
        if (!IsAllowed(url))
            return;
        _wv.Url = url;
    }

    private static bool IsAllowed(string url)
    {
        // Allow internal CEF protocols used during page rendering
        if (url.StartsWith("about:", StringComparison.Ordinal)
            || url.StartsWith("data:", StringComparison.Ordinal)
            || url.StartsWith("res://", StringComparison.Ordinal))
            return true;

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return false;

        return uri.Host.Equals(AllowedHost, StringComparison.OrdinalIgnoreCase)
               || uri.Host.EndsWith("." + AllowedHost, StringComparison.OrdinalIgnoreCase);
    }

    private static void OnBeforeBrowse(IBeforeBrowseContext ctx)
    {
        // Permit internal CEF navigations (about:blank, data: URIs used during render)
        if (ctx.Url.StartsWith("about:", StringComparison.Ordinal)
            || ctx.Url.StartsWith("data:", StringComparison.Ordinal))
            return;

        if (!IsAllowed(ctx.Url))
            ctx.DoCancel();
    }
}
