﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CoreWiki.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.TagHelpers;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Localization;

namespace CoreWiki.TagHelpers;

/// <summary>
/// <see cref="ITagHelper"/> implementation targeting &lt;pager&gt; elements.
/// </summary>
/// <remarks>
/// Generates a bootstrap 4 pagination block.
/// </remarks>
[HtmlTargetElement("pager")]
public class PagerTagHelper : TagHelper
{
	private readonly IHtmlGenerator                   _Generator;
	private readonly IStringLocalizer<PagerTagHelper> localizer;

	public PagerTagHelper(IHtmlGenerator generator, IStringLocalizer<PagerTagHelper> localizer)
	{
		_Generator     = generator;
		this.localizer = localizer;
	}

	public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
	{
		output.TagMode = TagMode.StartTagAndEndTag;
		output.TagName = "ul";

		var ul = new TagBuilder("ul");
		ul.AddCssClass("pagination");
		output.MergeAttributes(ul);

		if (CurrentPage < 1) CurrentPage = 1;

		AppendPreNavigationButtons(output);
		AppendNavigationButtons(output);
		AppendPostNavigationButtons(output);

		await base.ProcessAsync(context, output);
		return;

	}

	private Dictionary<string, string> MakeRouteValues(int pageNumber)
	{
		var route = new Dictionary<string, string>
		{
			{"PageNumber", pageNumber.ToString()}
		};

		if (UrlParams == null)
		{
			return route;
		}

		foreach (var key in UrlParams.Keys)
		{
			// We don't want to override existing values such as PageNumber
			if (route.ContainsKey(key))
			{
				continue;
			}
			route.Add(key, UrlParams[key]);
		}

		return route;
	}

	public TagBuilder CreatePageItem()
	{
		var tag = new TagBuilder("li");
		tag.AddCssClass("page-item");
		return tag;
	}

	private TagBuilder CreatePaginatorButton(string textForScreenReaders, string textToDisplay, int pageNum, bool clickable)
	{
		var tag = clickable
			? _Generator.GeneratePageLink(
				ViewContext,
				linkText: "",
				pageName: AspPage,
				pageHandler: string.Empty,
				protocol: string.Empty,
				hostname: string.Empty,
				fragment: string.Empty,
				routeValues: MakeRouteValues(pageNum),
				htmlAttributes: null
			)
			: new TagBuilder("span");
		tag.AddCssClass("page-link");
		tag.AddAriaSpans(textForScreenReaders, textToDisplay);
		return tag;
	}

	private void AppendPreNavigationButtons(TagHelperOutput output)
	{
		var clickable = CurrentPage > 1;
		if (!AreNavigationButtonsVisible(clickable)) return;

		if (AreFirstAndLastButtonsVisible())
		{
			var first = CreatePageItem();
			first.InnerHtml.AppendHtml(CreatePaginatorButton(localizer["FirstPage"], "<<", 1, clickable));

			if (!clickable)
			{
				first.AddCssClass("disabled");
			}

			output.Content.AppendHtml(first);
		}

		if (AreNextAndPreviousButtonsVisible())
		{
			var previous           = CreatePageItem();
			var previousPageNumber = CurrentPage - 1 > TotalPages ? TotalPages : CurrentPage - 1;

			previous.InnerHtml.AppendHtml(CreatePaginatorButton(localizer["PreviousPage"], "<", previousPageNumber,
				clickable));

			if (!clickable)
			{
				previous.AddCssClass("disabled");
			}

			output.Content.AppendHtml(previous);
		}
	}

	private void AppendPostNavigationButtons(TagHelperOutput output)
	{
		var clickable = TotalPages > 0 && CurrentPage < TotalPages;
		if (!AreNavigationButtonsVisible(clickable)) return;

		if (AreNextAndPreviousButtonsVisible())
		{
			var next           = CreatePageItem();
			var nextPageNumber = CurrentPage < 1 ? 1 : CurrentPage + 1;
			next.InnerHtml.AppendHtml(CreatePaginatorButton(localizer["NextPage"], ">", nextPageNumber, clickable));

			if (!clickable)
			{
				next.AddCssClass("disabled");
			}

			output.Content.AppendHtml(next);
		}

		if (AreFirstAndLastButtonsVisible())
		{
			var last = CreatePageItem();
			last.InnerHtml.AppendHtml(CreatePaginatorButton(localizer["LastPage"], ">>", TotalPages, clickable));

			if (!clickable)
			{
				last.AddCssClass("disabled");
			}

			output.Content.AppendHtml(last);
		}
	}

	private bool AreNavigationButtonsVisible(bool clickable)
	{
		if (NavigationButtonVisibility == ButtonVisibility.Never) return false;
		if (NavigationButtonVisibility == ButtonVisibility.Auto && !clickable) return false;
		if (NavigationButtons == NavigationButtonChoices.None) return false;
		return true;
	}

	private bool AreNextAndPreviousButtonsVisible()
	{
		return NavigationButtons == NavigationButtonChoices.All ||
		       NavigationButtons == NavigationButtonChoices.OnlyNextAndPrevious;
	}

	private bool AreFirstAndLastButtonsVisible()
	{
		return NavigationButtons == NavigationButtonChoices.All ||
		       NavigationButtons == NavigationButtonChoices.OnlyFirstAndLast;
	}

	private void AppendNavigationButtons(TagHelperOutput output)
	{
		var (start, end) = CalculatePaginatorDisplayRange(CurrentPage, TotalPages, MaxPagesDisplayed);

		for (var pageNum = start; pageNum <= end; pageNum++)
		{
			var li = CreatePageItem();

			if (pageNum == CurrentPage)
			{
				li.AddCssClass("active");
				li.InnerHtml.AppendHtml(CreatePaginatorButton(localizer["CurrentPage"], $"{pageNum}", pageNum, false));
			}
			else
			{
				li.InnerHtml.AppendHtml(CreatePaginatorButton($"{localizer["Page"]} {pageNum}", $"{pageNum}", pageNum, true));
			}

			output.Content.AppendHtml(li);
		}
	}

	private (int start, int end) CalculatePaginatorDisplayRange(int currentPage, int totalPages, int maxPagesDisplayed)
	{
		var start = 0;
		var end   = 0;

		var midPoint                  = (int)Math.Floor(MaxPagesDisplayed / 2.0);
		var pagesToShowBeforeMidpoint = MaxPagesDisplayed - midPoint - 1;
		var pagesToShowAfterMidpoint  = MaxPagesDisplayed - pagesToShowBeforeMidpoint - 1;

		if (CurrentPage <= pagesToShowBeforeMidpoint)
		{
			start = 1;
			end   = Math.Min(MaxPagesDisplayed, TotalPages);
		}
		else if (CurrentPage >= TotalPages - pagesToShowBeforeMidpoint)
		{
			start = MaxPagesDisplayed > TotalPages ? 1 : TotalPages - MaxPagesDisplayed + 1;
			end   = TotalPages;
		}
		else
		{
			start = CurrentPage - pagesToShowBeforeMidpoint;
			end   = CurrentPage + pagesToShowAfterMidpoint;
		}

		return (start, end);
	}

	/// <summary>
	/// The name of the page.
	/// </summary>
	/// <remarks>
	/// Can be <c>null</c> if refering to the current page.
	/// </remarks>
	public string AspPage { get; set; }

	/// <summary>
	/// 	Optional. Enables adding url parameters (e.g '?query=test') to the link URL
	/// </summary>
	public Dictionary<string, string> UrlParams { get; set; }

	/// <summary>
	/// The number of the current page.
	/// </summary>
	/// <remarks>
	/// If not specified this will default to <c>1</c>.
	/// </remarks>
	public int CurrentPage { get; set; } = 1;

	/// <summary>
	/// The total number of page links available to show.
	/// </summary>
	/// <remarks>
	/// This is required and can not be <c>null</c>.
	/// </remarks>
	public int TotalPages { get; set; }

	/// <summary>
	/// Show up to this number of page links in the paginator.
	/// </summary>
	/// <remarks>
	/// If not specified this will default to <c>10</c>.
	/// </remarks>
	public int MaxPagesDisplayed { get; set; } = 10;

	/// <summary>
	/// Gets or sets whether navigation buttons will be displayed when they aren't necessary.
	/// </summary>
	/// <remarks>
	///	If not specified this will default to <c>Always</c>
	/// </remarks>
	public ButtonVisibility NavigationButtonVisibility { get; set; } = ButtonVisibility.Always;

	/// <summary>
	/// Gets or sets which navigation buttons will be displayed
	/// </summary>
	/// <remarks>
	/// If not specified this will default to <c>All</c>
	/// </remarks>
	public NavigationButtonChoices NavigationButtons { get; set; } = NavigationButtonChoices.All;
		
	/// <summary>
	/// Gets or sets the <see cref="Rendering.ViewContext"/> for the current request.
	/// </summary>
	[HtmlAttributeNotBound]
	[ViewContext]
	public ViewContext ViewContext { get; set; }
}

public enum ButtonVisibility
{
	Auto,
	Always,
	Never
}

public enum NavigationButtonChoices
{
	All,
	None,
	OnlyNextAndPrevious,
	OnlyFirstAndLast
}