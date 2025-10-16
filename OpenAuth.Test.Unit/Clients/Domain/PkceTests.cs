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
            var challenge = Base64UrlEncoder.Encode(SHA256.HashData(Encoding.UTF8.GetBytes(verifier)));
            var pkce = Pkce.Create(challenge, CodeChallengeMethod.S256);

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