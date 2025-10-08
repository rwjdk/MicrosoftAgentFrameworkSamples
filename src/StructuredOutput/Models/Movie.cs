// Copyright (c) Microsoft. All rights reserved.

namespace StructuredOutput.Models;

public class Movie
{
    public required string Title { get; set; }
    public int YearOfRelease { get; set; }
    public required string Director { get; set; }
    public MovieGenre Genre { get; set; }
    public decimal ImdbScore { get; set; }
}