namespace PMCSsE_Communicator
{
    internal enum HandShakeProcess_Server
    {
        Beginning,
        WaitingNeedRSAPublicKey,
        ReceivedNeedRSAPublicKey,
        SendingRSAPublicKey,
        SentRSAPublicKey,
        WaitingGotRSAPublicKey,
        ReceivedGotRSAPublicKey,
        SendingNeedAES,
        SentNeedAES,
        WaitingAESKey,
        ReceivedAESKey,
        SendingGotAES,
        SentGotAES,
        WaitingLogin,
        ReceivedLogin,
        SendingSucceed,
        Finished
    }
    internal enum HandShakeProcess_Client
    {
        Beginning, 
        SendingNeedRSAPublicKey,
        SentNeedRSAPublicKey,
        WaitingRSAPublicKey,
        ReceivedRSAPublicKey,
        SendingGotRSAPublicKey,
        SentGotRSAPublicKey,
        Finished
    }

}