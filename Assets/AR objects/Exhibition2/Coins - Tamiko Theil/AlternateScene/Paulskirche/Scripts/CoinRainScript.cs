using System;
using System.Collections.Generic;
using UnityEngine;

public class CoinRainScript : MonoBehaviour
{
    public GameObject Coin1;
    public GameObject Coin2;
    public GameObject Coin3;
    public GameObject Coin4;
    public GameObject Coin5;
    public GameObject Coin6;

    public int _xOffset = 30;
    public int _dropHeight = 10;
    public int _areaSize = 10;
    public int _startSecond = 0;
    public int _endSecond = 0;

    public float _numberOfNewCoinsPerSecond = 1.5f;

    private readonly List<GameObject> _coins = new List<GameObject>();
    private long _startMilliSecond;
    private int _numberOfCoins = 0;

    protected void Start()
    {
        _startMilliSecond = DateTime.Now.Ticks / 10000;

        if (Coin1 != null)
        {
            _coins.Add(Coin1);
        }
        if (Coin2 != null)
        {
            _coins.Add(Coin2);
        }
        if (Coin3 != null)
        {
            _coins.Add(Coin3);
        }
        if (Coin4 != null)
        {
            _coins.Add(Coin4);
        }
        if (Coin5 != null)
        {
            _coins.Add(Coin5);
        }
        if (Coin6 != null)
        {
            _coins.Add(Coin6);
        }
    }

    protected void Update()
    {
        if (_coins.Count < 1)
        {
            return;
        }
        long milliSecond = DateTime.Now.Ticks / 10000 - _startMilliSecond;

        if (milliSecond / 1000 < _startSecond)
        {
            return;
        }
        if (milliSecond / 1000 > _endSecond)
        {
            return;
        }

        while (_numberOfCoins < (milliSecond - _startSecond * 1000) * _numberOfNewCoinsPerSecond / 1000)
        {
            Vector3 position = new Vector3(
                UnityEngine.Random.Range(-1000 * _areaSize / 2, 1000 * _areaSize / 2) / 1000.0f + _xOffset,
                UnityEngine.Random.Range(-1000 * _dropHeight / 5, 1000 * _dropHeight / 5) / 1000.0f + _dropHeight,
                UnityEngine.Random.Range(-1000 * _areaSize / 2, 1000 * _areaSize / 2) / 1000.0f
                );

            GameObject newCoin = Instantiate(_coins[_numberOfCoins++ % _coins.Count], position, UnityEngine.Random.rotation);

            newCoin.transform.parent = gameObject.transform;
            newCoin.SetActive(true);
        }
    }
}

