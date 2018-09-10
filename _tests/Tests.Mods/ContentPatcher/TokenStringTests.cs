using System.Linq;
using ContentPatcher.Framework.Conditions;
using ContentPatcher.Framework.Tokens;
using FluentAssertions;
using NUnit.Framework;
using Pathoschild.Stardew.Common.Utilities;
using Pathoschild.Stardew.Tests.Mods.ContentPatcher.Framework;

namespace Pathoschild.Stardew.Tests.Mods.ContentPatcher
{
    /// <summary>Unit tests for <see cref="TokenString"/>.</summary>
    [TestFixture]
    // ReSharper disable once InconsistentNaming
    internal class TokenStringTests
    {
        /*********
        ** Unit tests
        *********/
        /// <summary>Test that the <see cref="TokenString"/> constructor sets the expected property values when given a string not containing any tokens.</summary>
        [TestCase("")]
        [TestCase("   ")]
        [TestCase("boop")]
        [TestCase("  assets/boop.png  ")]
        public void TokenStringBuilder_PlainString(string raw)
        {
            // act
            TokenString tokenStr = new TokenString(raw, new MockContext());

            // assert
            tokenStr.Raw.Should().Be(raw.Trim());
            tokenStr.Tokens.Should().HaveCount(0);
            tokenStr.InvalidTokens.Should().HaveCount(0);
            tokenStr.HasAnyTokens.Should().BeFalse();
        }

        /// <summary>Test that the <see cref="TokenString"/> constructor sets the expected property values when given a single valid token.</summary>
        [TestCase("{{tokenKey}}")]
        [TestCase("  {{tokenKey}}   ")]
        [TestCase("  {{  tokenKey  }}   ")]
        public void TokenStringBuilder_WithSingleToken(string raw)
        {
            // arrange
            const string configKey = "tokenKey";
            var context = new MockContext(
                new StaticToken(configKey, canHaveMultipleValues: true, values: new InvariantHashSet { "value" })
            );

            // act
            TokenString tokenStr = new TokenString(raw, context);

            // assert
            tokenStr.Raw.Should().Be(raw.Trim());
            tokenStr.Tokens.Should().HaveCount(1);
            tokenStr.Tokens.Select(p => p.Name).Should().BeEquivalentTo(configKey);
            tokenStr.InvalidTokens.Should().BeEmpty();
            tokenStr.HasAnyTokens.Should().BeTrue();
            tokenStr.IsSingleTokenOnly.Should().BeTrue();
        }

        /// <summary>Test that the <see cref="TokenString"/> constructor sets the expected property values when given a string containing config, condition, and invalid values.</summary>
        [TestCase]
        public void TokenStringBuilder_WithTokens()
        {
            // arrange
            const string configKey = "configKey";
            const string configValue = "A";
            const string tokenKey = "season";
            const string raw = "  assets/{{configKey}}_{{season}}_{{invalid}}.png  ";
            var context = new MockContext(
                new StaticToken(configKey, canHaveMultipleValues: true, values: new InvariantHashSet { configValue }),
                new StaticToken(tokenKey, canHaveMultipleValues: false, values: new InvariantHashSet { "A" })
            );

            // act
            TokenString tokenStr = new TokenString(raw, context);

            // assert
            tokenStr.Raw.Should().Be(raw.Trim());
            tokenStr.Tokens.Should().HaveCount(2);
            tokenStr.Tokens.Select(p => p.Name).Should().BeEquivalentTo(new[] { configKey, tokenKey });
            tokenStr.InvalidTokens.Should().HaveCount(1).And.BeEquivalentTo("invalid");
            tokenStr.HasAnyTokens.Should().BeTrue();
            tokenStr.IsSingleTokenOnly.Should().BeFalse();
        }
    }
}
