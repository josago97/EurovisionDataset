using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EurovisionDataset.Scrapers.Senior;

internal class Ogaespain : BaseOgaespain
{
    protected override string LogosFolderName => "senior";

    protected override string GetPageUrl(int year)
    {
        throw new NotImplementedException();
    }
}
