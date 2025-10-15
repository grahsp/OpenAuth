using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using OpenAuth.Domain.AuthorizationGrants.Enums;
using OpenAuth.Domain.AuthorizationGrants.ValueObjects;

namespace OpenAuth.Test.Unit.Clients.Domain;

public class PkceTests
{
    public class Create
    {
        [Fact]
        public void WithPlainMethod_SetsRawVerifierAsChallenge()
        {
            // Arrange
            const string verifier = "plain-verifier";

            // Act
            var pkce = Pkce.Create(verifier, CodeChallengeMethod.Plain);

            // Assert
            Assert.Equal(verifier, pkce.CodeChallenge);
            Assert.Equal(CodeChallengeMethod.Plain, pkce.CodeChallengeMethod);
        }

        [Fact]
        public void WithS256Method_ComputesExpectedChallenge()
        {
            // Arrange
            const string verifier = "test-verifier";
            var expected = Base64UrlEncoder.Encode(SHA256.HashData(Encoding.UTF8.GetBytes(verifier)));

            // Act
            var pkce = Pkce.Create(verifier, CodeChallengeMethod.S256);

            // Assert
            Assert.Equal(expected, pkce.CodeChallenge);
            Assert.Equal(CodeChallengeMethod.S256, pkce.CodeChallengeMethod);
        }

        [Fact]
        public void WithUnsupportedMethod_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            const string verifier = "verifier";
            const CodeChallengeMethod invalidMethod = (CodeChallengeMethod)99;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(
                () => Pkce.Create(verifier, invalidMethod));
        }
    }


    public class Matches
    {
        [Fact]
        public void WithPlainMethod_AndSameVerifier_ReturnsTrue()
        {
            // Arrange
            const string verifier = "same-verifier";
            var pkce = Pkce.Create(verifier, CodeChallengeMethod.Plain);

            // Act
            var result = pkce.Matches(verifier);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void WithPlainMethod_AndDifferentVerifier_ReturnsFalse()
        {
            // Arrange
            var pkce = Pkce.Create("expected", CodeChallengeMethod.Plain);

            // Act
            var result = pkce.Matches("wrong");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void WithS256Method_AndCorrectVerifier_ReturnsTrue()
        {
            // Arrange
            const string verifier = "test-verifier";
            var pkce = Pkce.Create(verifier, CodeChallengeMethod.S256);

            // Act
            var result = pkce.Matches(verifier);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void WithS256Method_AndIncorrectVerifier_ReturnsFalse()
        {
            // Arrange
            var pkce = Pkce.Create("correct", CodeChallengeMethod.S256);

            // Act
            var result = pkce.Matches("wrong");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void WithEmptyVerifier_ReturnsFalse()
        {
            // Arrange
            var pkce = Pkce.Create("something", CodeChallengeMethod.S256);

            // Act
            var result = pkce.Matches(string.Empty);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void WithWhitespaceVerifier_ReturnsFalse()
        {
            // Arrange
            var pkce = Pkce.Create("something", CodeChallengeMethod.Plain);

            // Act
            var result = pkce.Matches("   ");

            // Assert
            Assert.False(result);
        }
    }
}