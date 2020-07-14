using System;
using System.Collections;
using System.Collections.Generic;

public class Message
{
    private DateTime hitTime;
    private int userID;

    public Message(int userID)
    {
        this.userID = userID;
        hitTime = DateTime.Now;
    }

    public DateTime GetTime()
    {
        return hitTime;
    }
}
