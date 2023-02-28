using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIOne : MonoBehaviour
{
    private HttpTest httpTest;
    private InputField uriInput;
    [SerializeField] private Button btnCommit;

    private void Awake()
    {
        uriInput = GetComponent<InputField>();
    }

    private void OnEnable()
    {
        btnCommit.onClick.AddListener(Commit);
    }

    private void OnDisable()
    {
        btnCommit.onClick.RemoveAllListeners();
    }

    void Commit()
    {
        //未校验
        httpTest.PrintHeaderAndBody(uriInput.text);
    }
}
