using CryptoPro.Security.Cryptography.Pkcs;
using CryptoPro.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Pkcs;
using iText.Kernel.Pdf;
using iText.Signatures;

internal class SignatureContainer : IExternalSignatureContainer
{
    private readonly CpX509Certificate2 _cert;

    internal SignatureContainer(CpX509Certificate2 cert)
    {
        this._cert = cert;
    }

    public virtual byte[] Sign(Stream docStream)
    {
        byte[] docBytes = StreamToByteArray(docStream);
        // Вычисляем подпись
        ContentInfo contentInfo = new(docBytes);
        CpSignedCms signedCms = new(contentInfo, true);
        //CpCmsSigner cmsSigner = new CpCmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, _cert, _cert.PrivateKey);
        CpCmsSigner cmsSigner = new(_cert);
        signedCms.ComputeSignature(cmsSigner, false);
        byte[] pk = signedCms.Encode();
        return pk;
    }

    public virtual void ModifySigningDictionary(PdfDictionary signDic)
    {
        signDic.Put(PdfName.Filter, new PdfName("CryptoPro PDF"));
        signDic.Put(PdfName.SubFilter, PdfName.Adbe_pkcs7_detached);
    }

    private static byte[] StreamToByteArray(Stream sourceStream)
    {
        // If the source is already a MemoryStream, we can potentially use ToArray directly for efficiency, 
        // but CopyTo is more general for all stream types.
        using (MemoryStream memoryStream = new())
        {
            // Use CopyTo to transfer data from the source stream to the MemoryStream
            sourceStream.CopyTo(memoryStream);
            // Return the byte array from the MemoryStream
            return memoryStream.ToArray();
        }
    }
}