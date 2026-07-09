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
    public enum HandShakeProcess_Client
    {
        Beginning,
        SendingNeedRSAPublicKey,
        SentNeedRSAPublicKey,
        WaitingRSAPublicKey,
        ReceivedRSAPublicKey,
        SendingGotRSAPublicKey,
        SentGotRSAPublicKey,
        WaitingNeedAES,
        ReceivedNeedAES,
        SendingAESKey,
        SentAESKey,
        WaitingGotAES,
        ReceivedGotAES,
        SendingLogin,
        SentLogin,
        WaitingSucceed,
        Finished
    }

}