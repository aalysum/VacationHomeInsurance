using System.Globalization;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using VacationHome.PolicyService.Application.Configuration;
using VacationHome.PolicyService.Application.Interfaces.Services;
using VacationHome.PolicyService.Domain.Entities;

namespace VacationHome.PolicyService.Infrastructure.Services
{
    public sealed class PolicyPdfGenerator : IPolicyPdfGenerator
    {
        private const string AcademicDemonstrationNotice =
            "For academic demonstration purposes only — not valid insurance coverage.";

        private static readonly XColor DarkSlateGray =
            XColor.FromArgb(47, 79, 79);

        private static readonly XColor Gainsboro =
            XColor.FromArgb(220, 220, 220);

        private static readonly XColor WhiteSmoke =
            XColor.FromArgb(245, 245, 245);

        public byte[] Generate(
            Policy policy,
            int version,
            DateTime generatedAtUtc,
            PolicyDocumentOptions options)
        {
            ArgumentNullException.ThrowIfNull(policy);
            ArgumentNullException.ThrowIfNull(options);

            if (string.IsNullOrWhiteSpace(options.ProviderName) ||
                string.IsNullOrWhiteSpace(options.PolicyWording))
            {
                throw new InvalidOperationException(
                    "PolicyDocuments:ProviderName and PolicyDocuments:PolicyWording must be configured before issuing policy documents.");
            }

            using var document = new PdfDocument();
            document.Info.Title =
                $"Vacation Home Insurance Policy {policy.PolicyNumber}";
            document.Info.Subject =
                "Policy certificate and coverage schedule";

            var layout = new PolicyDocumentLayout(
                document,
                options.ProviderName.Trim(),
                policy.PolicyNumber);
            layout.AddTitle(
                "VACATION HOME INSURANCE POLICY",
                19,
                bold: true,
                spaceAfter: 4);
            layout.AddTitle(
                "POLICY CERTIFICATE AND COVERAGE SCHEDULE",
                9,
                bold: true,
                color: DarkSlateGray,
                spaceAfter: 14);
            layout.AddCenteredText(
                AcademicDemonstrationNotice,
                8.5,
                italic: true,
                color: DarkSlateGray,
                spaceAfter: 8);

            layout.AddSectionHeading(1, "Policy certificate");
            layout.AddKeyValueTable(
                new (string Label, string Value)[]
                {
                    ("Policy number", policy.PolicyNumber),
                    ("Customer", FullName(policy)),
                    ("Contact number", EmptyFallback(policy.CustomerPhoneNumber)),
                    ("Insured property", policy.PropertyName),
                    ("Property address", FormatAddress(policy)),
                    ("Product", policy.ProductName),
                    ("Policy type", policy.ProductType.ToString()),
                    ("Effective date", FormatDate(policy.EffectiveDateUtc)),
                    ("Expiry date", FormatDate(policy.ExpiryDateUtc)),
                    ("Issue date", FormatDate(policy.CreatedAtUtc)),
                    ("Annual premium", FormatCurrency(policy.AnnualPremium)),
                    ("Payment frequency", EmptyFallback(policy.PaymentFrequency))
                });

            layout.AddSectionHeading(2, "Coverage schedule");
            layout.AddCoverageTable(policy.Coverages);

            layout.AddSectionHeading(3, "Premium breakdown");
            layout.AddKeyValueTable(
                new (string Label, string Value)[]
                {
                    ("Base rate", FormatPercent(policy.BaseRate)),
                    ("Base premium", FormatCurrency(policy.BasePremium)),
                    ("Risk level", EmptyFallback(policy.RiskLevel)),
                    ("Risk multiplier", FormatMultiplier(policy.RiskMultiplier)),
                    ("Risk adjustment", FormatCurrency(policy.RiskAdjustmentAmount)),
                    ("Mandatory coverage charges", FormatCurrency(policy.MandatoryCoverageCharges)),
                    ("Optional coverage charges", FormatCurrency(policy.OptionalCoverageCharges)),
                    ("Final annual premium", FormatCurrency(policy.AnnualPremium))
                });

            layout.AddSectionHeading(4, "Property and risk information");
            layout.AddKeyValueTable(
                new (string Label, string Value)[]
                {
                    ("Property type", EmptyFallback(policy.PropertyType)),
                    ("Property value", FormatCurrency(policy.PropertyValue)),
                    ("Construction year", policy.ConstructionYear > 0 ? policy.ConstructionYear.ToString(CultureInfo.InvariantCulture) : "Not recorded at issuance"),
                    ("Property age at issuance", policy.PropertyAge > 0 ? $"{policy.PropertyAge} years" : "Not recorded at issuance"),
                    ("Occupancy frequency", EmptyFallback(policy.OccupancyFrequency)),
                    ("Security system", policy.HasSecuritySystem ? "Present" : "Not recorded as present"),
                    ("Previous claims", policy.PreviousClaimsCount.ToString(CultureInfo.InvariantCulture)),
                    ("Disaster exposure", policy.DisasterExposure ? "Recorded" : "Not recorded"),
                    ("Risk score", policy.RiskScore > 0 ? policy.RiskScore.ToString(CultureInfo.InvariantCulture) : "Not recorded at issuance"),
                    ("Risk explanation", EmptyFallback(policy.RiskExplanation))
                });

            layout.AddSectionHeading(5, "Sample terms and conditions");
            layout.AddConfiguredText(
                "Sample policy wording",
                options.PolicyWording);
            layout.AddConfiguredText("Covered events", options.CoveredEvents);
            layout.AddConfiguredText("Exclusions", options.Exclusions);
            layout.AddConfiguredText(
                "Claims instructions",
                options.ClaimsInstructions);
            layout.AddConfiguredText(
                "Cancellation conditions",
                options.CancellationTerms);
            layout.AddConfiguredText(
                "Renewal conditions",
                options.RenewalTerms);

            if (!string.IsNullOrWhiteSpace(options.ProviderContact))
            {
                layout.AddConfiguredText(
                    "Insurer contact",
                    options.ProviderContact);
            }

            layout.AddFooters(
                policy.PolicyNumber,
                GetVersionLabel(options, version),
                FormatDate(generatedAtUtc));

            using var stream = new MemoryStream();
            document.Save(stream, closeStream: false);
            return stream.ToArray();
        }

        private static string FullName(Policy policy)
        {
            string name =
                $"{policy.CustomerFirstName} {policy.CustomerLastName}".Trim();
            return EmptyFallback(name);
        }

        private static string FormatAddress(Policy policy)
        {
            return string.Join(
                ", ",
                new[]
                {
                    policy.PropertyAddressLine1,
                    policy.PropertyAddressLine2,
                    policy.PropertyCity,
                    policy.PropertyState,
                    policy.PropertyPostalCode,
                    policy.PropertyCountry
                }.Where(value => !string.IsNullOrWhiteSpace(value)));
        }

        private static string FormatCurrency(decimal amount)
        {
            return amount.ToString(
                "C2",
                CultureInfo.GetCultureInfo("en-IN"));
        }

        private static string FormatPercent(decimal value)
        {
            return value.ToString("P4", CultureInfo.InvariantCulture);
        }

        private static string FormatMultiplier(decimal value)
        {
            return value.ToString("0.####", CultureInfo.InvariantCulture);
        }

        private static string FormatDate(DateTime value)
        {
            return value.ToUniversalTime().ToString(
                "dd MMM yyyy",
                CultureInfo.InvariantCulture);
        }

        private static string GetVersionLabel(
            PolicyDocumentOptions options,
            int version)
        {
            return $"{options.DocumentVersionLabel}.{version}";
        }

        private static string EmptyFallback(string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? "Not recorded at issuance"
                : value.Trim();
        }

        private sealed class PolicyDocumentLayout
        {
            private const double PointsPerCentimeter = 72 / 2.54;
            private const double LeftMargin = 1.8 * PointsPerCentimeter;
            private const double RightMargin = 1.8 * PointsPerCentimeter;
            private const double TopMargin = 2.1 * PointsPerCentimeter;
            private const double BottomMargin = 1.8 * PointsPerCentimeter;

            private readonly PdfDocument _document;
            private readonly string _providerName;
            private readonly string _policyNumber;
            private PdfPage _page = null!;
            private XGraphics _graphics = null!;
            private double _y;

            public PolicyDocumentLayout(
                PdfDocument document,
                string providerName,
                string policyNumber)
            {
                _document = document;
                _providerName = providerName;
                _policyNumber = policyNumber;
                StartPage();
            }

            private double ContentWidth =>
                _page.Width.Point - LeftMargin - RightMargin;

            private double ContentBottom =>
                _page.Height.Point - BottomMargin - 13;

            public void AddTitle(
                string text,
                double fontSize,
                bool bold,
                XColor? color = null,
                double spaceAfter = 0)
            {
                XFont font = CreateFont(fontSize, bold);
                IReadOnlyList<string> lines =
                    WrapText(text, font, ContentWidth);
                double lineHeight = fontSize * 1.3;
                double height = lines.Count * lineHeight;
                EnsureSpace(height + spaceAfter);

                DrawLines(
                    lines,
                    font,
                    color ?? XColors.Black,
                    LeftMargin,
                    _y,
                    ContentWidth,
                    lineHeight,
                    centered: true);
                _y += height + spaceAfter;
            }

            public void AddCenteredText(
                string text,
                double fontSize,
                bool italic,
                XColor color,
                double spaceAfter)
            {
                XFont font = CreateFont(fontSize, italic: italic);
                IReadOnlyList<string> lines =
                    WrapText(text, font, ContentWidth);
                double lineHeight = fontSize * 1.3;
                double height = lines.Count * lineHeight;
                EnsureSpace(height + spaceAfter);

                DrawLines(
                    lines,
                    font,
                    color,
                    LeftMargin,
                    _y,
                    ContentWidth,
                    lineHeight,
                    centered: true);
                _y += height + spaceAfter;
            }

            public void AddSectionHeading(int number, string text)
            {
                const double fontSize = 12;
                const double lineHeight = 15;
                EnsureSpace(13 + lineHeight + 5 + 24);
                _y += 13;
                _graphics.DrawString(
                    $"{number:00}  {text}",
                    CreateFont(fontSize, bold: true),
                    new XSolidBrush(DarkSlateGray),
                    new XPoint(LeftMargin, _y + fontSize));
                _y += lineHeight + 5;
            }

            public void AddKeyValueTable(
                IReadOnlyCollection<(string Label, string Value)> entries)
            {
                double labelWidth = 5.1 * PointsPerCentimeter;
                double valueWidth = 12.5 * PointsPerCentimeter;
                XFont normal = CreateFont(9);
                XFont labelFont = CreateFont(9, bold: true);

                foreach ((string label, string value) in entries)
                {
                    IReadOnlyList<string> labelLines =
                        WrapText(label, labelFont, labelWidth - 8);
                    IReadOnlyList<string> valueLines =
                        WrapText(
                            EmptyFallback(value),
                            normal,
                            valueWidth - 8);
                    int lineCount = Math.Max(
                        labelLines.Count,
                        valueLines.Count);
                    double rowHeight = lineCount * 11.5 + 8;
                    EnsureSpace(rowHeight);

                    double x = LeftMargin;
                    DrawTableCell(
                        x,
                        _y,
                        labelWidth,
                        rowHeight,
                        WhiteSmoke);
                    DrawTableCell(
                        x + labelWidth,
                        _y,
                        valueWidth,
                        rowHeight,
                        XColors.White);
                    DrawLines(
                        labelLines,
                        labelFont,
                        DarkSlateGray,
                        x + 4,
                        _y + 4,
                        labelWidth - 8,
                        11.5);
                    DrawLines(
                        valueLines,
                        normal,
                        XColors.Black,
                        x + labelWidth + 4,
                        _y + 4,
                        valueWidth - 8,
                        11.5);
                    _y += rowHeight;
                }
            }

            public void AddCoverageTable(
                IEnumerable<PolicyCoverage> coverages)
            {
                const double headerHeight = 27;
                double[] widths =
                [
                    2.3 * PointsPerCentimeter,
                    7.2 * PointsPerCentimeter,
                    3.6 * PointsPerCentimeter,
                    4.3 * PointsPerCentimeter
                ];
                string[] headings =
                [
                    "Type",
                    "Coverage",
                    "Limit",
                    "Annual charge"
                ];
                XFont headerFont = CreateFont(9, bold: true);
                IReadOnlyList<PolicyCoverage> orderedCoverages =
                    coverages.OrderBy(item => item.CoverageName).ToArray();

                EnsureSpace(headerHeight + 24);
                DrawCoverageHeader(widths, headings, headerFont, headerHeight);

                if (orderedCoverages.Count == 0)
                {
                    const double emptyRowHeight = 21;
                    EnsureSpace(
                        emptyRowHeight,
                        () => DrawCoverageHeader(
                            widths,
                            headings,
                            headerFont,
                            headerHeight));
                    DrawTableCell(
                        LeftMargin,
                        _y,
                        widths.Sum(),
                        emptyRowHeight,
                        XColors.White);
                    DrawLines(
                        ["No coverage details were recorded."],
                        CreateFont(9),
                        XColors.Black,
                        LeftMargin + 4,
                        _y + 5,
                        widths.Sum() - 8,
                        11.5);
                    _y += emptyRowHeight;
                    return;
                }

                int rowNumber = 0;
                foreach (PolicyCoverage coverage in orderedCoverages)
                {
                    string description =
                        string.IsNullOrWhiteSpace(coverage.CoverageDescription)
                            ? coverage.CoverageName
                            : $"{coverage.CoverageName}\n{coverage.CoverageDescription}";
                    string[] values =
                    [
                        coverage.IsMandatory ? "Mandatory" : "Optional",
                        description,
                        FormatCurrency(coverage.CoverageLimitAmount),
                        FormatCurrency(coverage.CoverageCharge)
                    ];
                    XFont normal = CreateFont(9);
                    IReadOnlyList<string>[] lines = values
                        .Select(
                            (text, index) => WrapText(
                                text,
                                normal,
                                widths[index] - 8))
                        .ToArray();
                    double rowHeight =
                        lines.Max(item => item.Count) * 11.5 + 10;
                    EnsureSpace(
                        rowHeight,
                        () => DrawCoverageHeader(
                            widths,
                            headings,
                            headerFont,
                            headerHeight));

                    XColor background = rowNumber++ % 2 == 0
                        ? XColors.White
                        : WhiteSmoke;
                    double x = LeftMargin;
                    for (int index = 0; index < widths.Length; index++)
                    {
                        DrawTableCell(
                            x,
                            _y,
                            widths[index],
                            rowHeight,
                            background);
                        DrawLines(
                            lines[index],
                            normal,
                            XColors.Black,
                            x + 4,
                            _y + 5,
                            widths[index] - 8,
                            11.5);
                        x += widths[index];
                    }

                    _y += rowHeight;
                }
            }

            public void AddConfiguredText(string heading, string value)
            {
                XFont labelFont = CreateFont(9, bold: true);
                XFont normal = CreateFont(9);
                string content = string.IsNullOrWhiteSpace(value)
                    ? "Not supplied."
                    : value.Trim();
                IReadOnlyList<string> lines =
                    WrapText(content, normal, ContentWidth);
                double contentHeight = Math.Max(1, lines.Count) * 11.5;
                double headingHeight = 7 + 15;
                double fullBlockHeight = headingHeight + 2 + contentHeight;
                bool fitsOnOnePage =
                    fullBlockHeight <= ContentBottom - TopMargin;
                EnsureSpace(
                    fitsOnOnePage
                        ? fullBlockHeight
                        : headingHeight + 2 + 11.5);
                _y += 7;
                _graphics.DrawString(
                    heading,
                    labelFont,
                    new XSolidBrush(XColors.Black),
                    new XPoint(LeftMargin, _y + 9));
                _y += 15;
                _y += 2;

                if (fitsOnOnePage)
                {
                    DrawLines(
                        lines,
                        normal,
                        XColors.Black,
                        LeftMargin,
                        _y,
                        ContentWidth,
                        11.5);
                    _y += contentHeight;
                    return;
                }

                foreach (string line in lines)
                {
                    EnsureSpace(11.5);
                    DrawLines(
                        [line],
                        normal,
                        XColors.Black,
                        LeftMargin,
                        _y,
                        ContentWidth,
                        11.5);
                    _y += 11.5;
                }
            }

            public void AddFooters(
                string policyNumber,
                string version,
                string issueDate)
            {
                _graphics.Dispose();

                for (int index = 0; index < _document.PageCount; index++)
                {
                    PdfPage page = _document.Pages[index];
                    using XGraphics graphics =
                        XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);
                    double center = page.Width.Point / 2;
                    double footerY = page.Height.Point - 43;
                    string footer =
                        $"Policy {policyNumber}  |  Version {version}  |  Issued {issueDate}  |  Page {index + 1} of {_document.PageCount}";
                    graphics.DrawString(
                        footer,
                        CreateFont(8),
                        XBrushes.Black,
                        new XPoint(center, footerY),
                        XStringFormats.TopCenter);
                    graphics.DrawString(
                        AcademicDemonstrationNotice,
                        CreateFont(7.5, italic: true),
                        XBrushes.Black,
                        new XPoint(center, footerY + 11),
                        XStringFormats.TopCenter);
                }
            }

            private void StartPage()
            {
                _page = _document.AddPage();
                _page.Size = PageSize.A4;
                _graphics = XGraphics.FromPdfPage(_page);
                DrawBrandHeader();
                _y = TopMargin;
            }

            private void DrawBrandHeader()
            {
                const double headerY = 23;
                double markWidth = 1.8 * PointsPerCentimeter;
                double providerWidth = 9.0 * PointsPerCentimeter;
                double referenceWidth = 6.6 * PointsPerCentimeter;
                double headerHeight = 1.25 * PointsPerCentimeter;
                _graphics.DrawRectangle(
                    new XSolidBrush(DarkSlateGray),
                    LeftMargin,
                    headerY,
                    markWidth,
                    headerHeight);
                _graphics.DrawString(
                    "SS",
                    CreateFont(16, bold: true),
                    XBrushes.White,
                    new XRect(
                        LeftMargin,
                        headerY,
                        markWidth,
                        headerHeight),
                    XStringFormats.Center);

                double providerX = LeftMargin + markWidth;
                _graphics.DrawString(
                    _providerName,
                    CreateFont(14, bold: true),
                    new XSolidBrush(DarkSlateGray),
                    new XPoint(providerX, headerY + 15));
                _graphics.DrawString(
                    "VACATION HOME INSURANCE",
                    CreateFont(8),
                    new XSolidBrush(DarkSlateGray),
                    new XPoint(providerX, headerY + 28));

                _graphics.DrawString(
                    $"POLICY  {_policyNumber}",
                    CreateFont(9, bold: true),
                    new XSolidBrush(DarkSlateGray),
                    new XRect(
                        providerX + providerWidth,
                        headerY,
                        referenceWidth,
                        headerHeight),
                    XStringFormats.TopRight);
            }

            private void DrawCoverageHeader(
                IReadOnlyList<double> widths,
                IReadOnlyList<string> headings,
                XFont font,
                double height)
            {
                double x = LeftMargin;
                for (int index = 0; index < widths.Count; index++)
                {
                    DrawTableCell(
                        x,
                        _y,
                        widths[index],
                        height,
                        DarkSlateGray);
                    _graphics.DrawString(
                        headings[index],
                        font,
                        XBrushes.White,
                        new XPoint(x + 4, _y + 6 + 9));
                    x += widths[index];
                }

                _y += height;
            }

            private void DrawTableCell(
                double x,
                double y,
                double width,
                double height,
                XColor fill)
            {
                _graphics.DrawRectangle(
                    new XPen(Gainsboro, 0.5),
                    new XSolidBrush(fill),
                    x,
                    y,
                    width,
                    height);
            }

            private void DrawLines(
                IReadOnlyList<string> lines,
                XFont font,
                XColor color,
                double x,
                double y,
                double width,
                double lineHeight,
                bool centered = false)
            {
                for (int index = 0; index < lines.Count; index++)
                {
                    XStringFormat format = centered
                        ? XStringFormats.TopCenter
                        : XStringFormats.TopLeft;
                    XPoint point = centered
                        ? new XPoint(x + width / 2, y + index * lineHeight)
                        : new XPoint(x, y + index * lineHeight);
                    _graphics.DrawString(
                        lines[index],
                        font,
                        new XSolidBrush(color),
                        point,
                        format);
                }
            }

            private IReadOnlyList<string> WrapText(
                string text,
                XFont font,
                double maxWidth)
            {
                var lines = new List<string>();
                string[] paragraphs = text
                    .Replace("\r\n", "\n", StringComparison.Ordinal)
                    .Replace('\r', '\n')
                    .Split('\n');

                foreach (string paragraph in paragraphs)
                {
                    string currentLine = string.Empty;
                    foreach (string word in paragraph.Split(
                                 ' ',
                                 StringSplitOptions.RemoveEmptyEntries))
                    {
                        string candidate = currentLine.Length == 0
                            ? word
                            : $"{currentLine} {word}";
                        if (currentLine.Length > 0 &&
                            _graphics.MeasureString(candidate, font).Width > maxWidth)
                        {
                            lines.Add(currentLine);
                            currentLine = word;
                        }
                        else
                        {
                            currentLine = candidate;
                        }
                    }

                    lines.Add(currentLine);
                }

                return lines;
            }

            private void EnsureSpace(
                double requiredHeight,
                Action? drawRepeatedHeader = null)
            {
                if (_y + requiredHeight <= ContentBottom)
                {
                    return;
                }

                _graphics.Dispose();
                StartPage();
                if (drawRepeatedHeader is not null)
                {
                    drawRepeatedHeader();
                }
            }

            private static XFont CreateFont(
                double size,
                bool bold = false,
                bool italic = false)
            {
                XFontStyleEx style = (bold, italic) switch
                {
                    (true, true) => XFontStyleEx.BoldItalic,
                    (true, false) => XFontStyleEx.Bold,
                    (false, true) => XFontStyleEx.Italic,
                    _ => XFontStyleEx.Regular
                };
                return new XFont("Arial", size, style);
            }
        }
    }
}
