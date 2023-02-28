using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class UITip : MonoBehaviour
{
   [SerializeField] private Text title;
   [SerializeField] private Text progress;
   [SerializeField] private Text percentage;
   [SerializeField] private Slider slider;
   [SerializeField] private Button close;
   public void UpdateInfo(long currentLength,long dataLength)
   {

      progress.text = $"{currentLength / 1024}kb/{dataLength / 1024}kb";
      percentage.text = $"{Math.Floor((float)currentLength/dataLength * 100)}%";
      slider.value = (float)currentLength/dataLength;
      if (currentLength < dataLength)
      {
         title.text = "下载完成";
         close.gameObject.SetActive(true);
      }
      else
      {
         title.text = "下载中";
         close.gameObject.SetActive(false);
      }
   }

   private void OnEnable()
   {
      close.onClick.AddListener(Hide);
   }

   private void OnDisable()
   {
      close.onClick.RemoveAllListeners();
   }

   public void Show() => gameObject.SetActive(true);
   public void Hide() => gameObject.SetActive(false);










}
