using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private const string _httpServer = "https://clickycratesproject.azurewebsites.net";
    public string HttpServer
    {
        get
        {
            return _httpServer;
        }
    }

    [SerializeField]
    string _token;
    public string Token
    {
        get { return _token; }
        set { _token = value; }
    }

    [SerializeField]
    string _id;
    public string Id
    {
        get { return _id; }
        set { _id = value; }
    }

    [SerializeField]
    string _firstName;
    public string FirstName
    {
        get { return _firstName; }
        set { _firstName = value; }
    }

    [SerializeField]
    string _lastName;
    public string LastName
    {
        get { return _lastName; }
        set { _lastName = value; }
    }

    [SerializeField]
    DateTime _dateOfBirth;
    public DateTime DateOfBirth
    {
        get { return _dateOfBirth; }
        set { _dateOfBirth = value; }
    }

    [SerializeField]
    string _nickName;
    public string NickName
    {
        get { return _nickName; }
        set { _nickName = value; }
    }

    [SerializeField]
    string _email;
    public string Email
    {
        get { return _email; }
        set { _email = value; }
    }

    [SerializeField]
    string _city;
    public string City
    {
        get { return _city; }
        set { _city = value; }
    }

    [SerializeField]
    DateTime _dateJoined;
    public DateTime DateJoined
    {
        get { return _dateJoined; }
        set { _dateJoined = value; }
    }

    [SerializeField]
    string _blobUri;
    public string BlobUri
    {
        get { return _blobUri; }
        set { _blobUri = value; }
    }

    [SerializeField]
    bool _login = false;
    public bool Login
    {
        get { return _login; }
        set { _login = value; }
    }

    private void Awake()
    {
        int count = FindObjectsOfType<Player>().Length;
        if (count > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }
}
