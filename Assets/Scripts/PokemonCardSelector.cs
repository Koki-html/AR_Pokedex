using System;
using UnityEngine;

public class PokemonCardSelector : MonoBehaviour
{
    [Serializable]
    private class CardContent
    {
        [SerializeField]
        private string referenceImageName;

        [SerializeField]
        private GameObject contentPrefab;

        [SerializeField]
        private Vector3 localPosition;

        [SerializeField]
        private Vector3 localEulerAngles;

        [SerializeField]
        private Vector3 localScale = Vector3.one;

        public bool Matches(string imageName)
        {
            return string.Equals(
                referenceImageName, imageName, StringComparison.Ordinal);
        }

        public GameObject ContentPrefab => contentPrefab;
        public Vector3 LocalPosition => localPosition;
        public Quaternion LocalRotation => Quaternion.Euler(localEulerAngles);
        public Vector3 LocalScale => localScale;
    }

    [SerializeField]
    private CardContent[] cardContents;

    public bool TryGetContent(
        string referenceImageName,
        out GameObject contentPrefab,
        out Vector3 localPosition,
        out Quaternion localRotation,
        out Vector3 localScale)
    {
        if (cardContents != null)
        {
            foreach (CardContent cardContent in cardContents)
            {
                if (cardContent == null || !cardContent.Matches(referenceImageName))
                    continue;

                contentPrefab = cardContent.ContentPrefab;
                localPosition = cardContent.LocalPosition;
                localRotation = cardContent.LocalRotation;
                localScale = cardContent.LocalScale;
                return true;
            }
        }

        contentPrefab = null;
        localPosition = Vector3.zero;
        localRotation = Quaternion.identity;
        localScale = Vector3.one;
        return false;
    }
}