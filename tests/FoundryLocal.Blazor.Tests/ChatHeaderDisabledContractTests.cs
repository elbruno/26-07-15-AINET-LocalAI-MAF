using System.Text.RegularExpressions;
using Xunit;

namespace FoundryLocal.Blazor.Tests;

/// <summary>
/// Regression guard for the <c>ChatHeader</c> action buttons' <c>disabled</c> markup.
///
/// The original markup used <c>disabled="@IsPreparingModel || IsModelReady"</c>.
/// Razor parses only <c>@IsPreparingModel</c> as the C# expression and treats the
/// trailing <c>|| IsModelReady</c> as LITERAL text, so the rendered attribute value
/// is a non-empty STRING. Blazor's boolean-attribute special-casing only omits a
/// <c>disabled</c> attribute when the bound value is a real <c>bool</c> that is
/// <c>false</c>; a non-empty string is always emitted, so the Prepare / Open-location
/// / Delete buttons were ALWAYS disabled. The fix wraps every multi-term condition in
/// <c>@( ... )</c> so it evaluates to a genuine <c>bool</c>.
///
/// These tests parse the real component source and enforce that contract on every
/// platform (CI runs on ubuntu; the RCL is win-x64/native, so a render test cannot
/// run there — a source-contract test can).
/// </summary>
public class ChatHeaderDisabledContractTests
{
    // Matches a full disabled="..."/disabled='...' attribute and captures its value.
    private static readonly Regex DisabledAttr =
        new("disabled\\s*=\\s*\"(?<val>[^\"]*)\"|disabled\\s*=\\s*'(?<val>[^']*)'",
            RegexOptions.Compiled);

    // Boolean operators that make an attribute value a multi-term expression.
    private static readonly Regex BooleanOperators =
        new("(\\|\\||&&|(?<![=!<>])!)", RegexOptions.Compiled);

    private static string ReadComponentSource()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", "ChatHeader.razor");
        Assert.True(File.Exists(path), $"Expected ChatHeader.razor copied to test output at {path}");
        return File.ReadAllText(path);
    }

    private static List<string> DisabledValues(string source) =>
        DisabledAttr.Matches(source).Select(m => m.Groups["val"].Value.Trim()).ToList();

    [Fact]
    public void ComponentSourceIsPresentForTest()
    {
        Assert.False(string.IsNullOrWhiteSpace(ReadComponentSource()));
    }

    [Fact]
    public void HasAtLeastThreeDisabledButtons()
    {
        // Prepare, Open-location, Delete, and Refresh all bind `disabled`.
        var values = DisabledValues(ReadComponentSource());
        Assert.True(values.Count >= 4, $"Expected >= 4 disabled bindings, found {values.Count}.");
    }

    [Fact]
    public void EveryDisabledBindingIsARazorExpression()
    {
        foreach (var value in DisabledValues(ReadComponentSource()))
        {
            Assert.StartsWith("@", value);
        }
    }

    [Fact]
    public void EveryMultiTermDisabledConditionIsWrappedInParens()
    {
        var offenders = new List<string>();

        foreach (var value in DisabledValues(ReadComponentSource()))
        {
            // Strip the wrapped form so operators inside @( ... ) don't count.
            var isWrapped = value.StartsWith("@(") && value.EndsWith(")");
            if (isWrapped)
            {
                continue;
            }

            // Unwrapped values (e.g. "@IsPreparingModel") are only safe if they are a
            // single bare identifier with NO boolean operators. Any operator here means
            // Razor would split expression/literal — the exact bug we are guarding.
            if (BooleanOperators.IsMatch(value))
            {
                offenders.Add(value);
            }
        }

        Assert.True(
            offenders.Count == 0,
            "Every multi-term `disabled` condition must be wrapped in @( ... ) so Razor " +
            "evaluates it to a real bool. Unwrapped multi-term conditions render as a " +
            "non-empty string and disable the button permanently. Offenders: " +
            string.Join(" | ", offenders));
    }
}
