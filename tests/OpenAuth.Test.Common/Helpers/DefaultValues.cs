using OpenAuth.Domain.OAuth;

namespace OpenAuth.Test.Common.Helpers;

public static class DefaultValues
{
    public const string ApplicationType = "web";

    public const string ClientName = "test-client";
    public const string ClientId = "855c5e72-8fb6-4dd0-88dd-5d830b7ccc60";
    public const string ClientSecret = "client-secret";

    public const string Code = "random-code";
    public const string GrantType = GrantTypes.AuthorizationCode;
    
    public const string ResponseType = "code";
    public const string Subject = "test-subject";
    
    public const string RedirectUri = "https://example.com/callback";
    public const string Audience = "api";
    public const string Scopes = "read write";

    public const string CodeVerifier = "code-verifier";
    public const string CodeChallengeMethod = "s256";

    public const string Nonce = "nonce";


    public const string UserId = "855c5e72-8fb6-4dd0-88dd-5d830b7ccc60";
    public const string UserName = "test-user";
    public const string UserEmail = "test-user@email.com";
    public const string UserPassword = "Test!23";


    public const string Secret = "this-is-a-very-secret-secret-that-must-not-be-made-public";
    public const string PublicPem = "-----BEGIN PUBLIC KEY-----\nMIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCHmjeOaesQAdi7MIMwzNtkYfvJ\n8SaFC3KCzh87D6l/QnRWuJAX4JG7BUnMAuuIPrEqlqVd0tTxUH9hwIP/SA9L6qok\nlB2ZJSTCTg4/q2pOlItG4bKa45pfGMEYydkRHoMbGuuVaTjpBGLsIHdOpQuNm17K\nWlkqtYrLt9ktbuUHuQIDAQAB\n-----END PUBLIC KEY-----\n";
    public const string PrivatePem = "-----BEGIN PRIVATE KEY-----\nMIICdgIBADANBgkqhkiG9w0BAQEFAASCAmAwggJcAgEAAoGBAIeaN45p6xAB2Lsw\ngzDM22Rh+8nxJoULcoLOHzsPqX9CdFa4kBfgkbsFScwC64g+sSqWpV3S1PFQf2HA\ng/9ID0vqqiSUHZklJMJODj+rak6Ui0bhsprjml8YwRjJ2REegxsa65VpOOkEYuwg\nd06lC42bXspaWSq1isu32S1u5Qe5AgMBAAECgYBUjCUjmIrFekFFxWOm47PPDQDO\n0prvzUlioV37lzJZdHfRMlY1bQGwGAYBO7jbRCt2oGMO8stugoBJ1Jz4aFeQVLFj\nDf5RKrCu6RiPVvEqht4v94Hzz7OYPL83c6bdsoveMAPA8aF40jZXqJtUWwrBwV83\npHvQNdZzlXQM907imQJBANrNetkCHiiCmaEgA+QRmveICrqndtpwy988hsOxHqyE\ny88xvu+zE5Ux6OUkHvfz9HcIxRIj9gqZki43NwULfVcCQQCep8aKi4qRXX9CUPrD\nhYy7izFKepL6d+zpgTx2ws8jtXSy0q3BkUu7Wpm1+Yc4hNEppIO8iiEsXDwBsnyp\n52lvAkA+62PuT5uYjqXbHbfAuAdWMzrSniGhg1o9IcynLaHifnWVaXq8t0RkXOva\nKN728qJUMKNrKggw2CSfQaWCv+EVAkAOhm224W+eP2EXQTK0E9X+lY+9sdsi8zfN\naPeQJ+Wu7z3v4TnnLtYwIOtRrbajhgVgOp+U81B2LzuPLDgdlgIhAkEAwn7BsU/x\n3MfbZSi1p0ZLo/BnO7PR+xy0ABigKYgyNYyrr5yiN4Pjg+rjT22e6qfDAOqHirWI\nBYA+8tmPdeZT/Q==\n-----END PRIVATE KEY-----";
}