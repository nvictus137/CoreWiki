﻿using MediatR;

namespace CoreWiki.Application.Articles.Reading.Commands;

public class IncrementViewCountCommand : IRequest<Unit>
{

	public IncrementViewCountCommand(string slug)
	{
		this.Slug = slug;
	}

	public string Slug { get; }

}
