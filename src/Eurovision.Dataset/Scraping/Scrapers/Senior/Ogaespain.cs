﻿using Eurovision.Dataset.Scraping.Scrapers;

namespace Eurovision.Dataset.Scraping.Scrapers.Senior;

internal class Ogaespain : BaseOgaespain
{
    protected override string GetPageUrl(int year)
    {
        return $"festival-de-eurovision/eurovision-{year}";
    }
}
