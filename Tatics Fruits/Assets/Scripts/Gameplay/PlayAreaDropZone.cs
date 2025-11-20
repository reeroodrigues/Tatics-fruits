using System.Collections.Generic;
using DG.Tweening;
using New_GameplayCore.Views;
using UnityEngine;
using UnityEngine.EventSystems;

namespace New_GameplayCore
{
    public class PlayAreaDropZone : MonoBehaviour, IDropHandler
    {
        [Header("Slots")]
        [SerializeField] private Transform dropContent;
        [SerializeField] private Transform matchedPile;

        [Header("Pile layout")]
        [SerializeField] private Vector2 pileStep = new Vector2(12f, -8f);
        [SerializeField] private float pileFanAngle = 8f;
        [SerializeField] private float pileScale = 0.9f;
        [SerializeField] private float pileAnimDuration = 0.2f;

        [Header("Refs")]
        [SerializeField] private GameControllerInitializer bootstrap;

        private IRuleEngine _rules;
        private IGameController _controller;
        private IHandService _hand;

        private readonly List<(CardView view, CardDragHandler drag, CardInstance data)> _staged = new(2);
        private int _pileCount = 0;

        void Start()
        {
            _rules = bootstrap.RuleEngine;
            _controller = bootstrap.Controller;
            _hand = bootstrap.Hand;

            if (!dropContent) dropContent = transform;
            if (!matchedPile) matchedPile = transform;
        }

        public void OnDrop(PointerEventData eventData)
        {
            var go = eventData.pointerDrag;
            if (go == null) return;

            var drag = go.GetComponent<CardDragHandler>();
            var view = go.GetComponent<CardView>();
            if (drag == null || view == null) return;

            var data = view.GetCardData();
        
            if (_staged.Count >= 2)
            {
                drag.ReturnToOrigin();
                return;
            }
        
            if (_staged.Count == 0)
            {
                _hand.TryRemove(data);
                drag.AcceptDrop(dropContent);
                PlayDropFlip(view.transform);
                _staged.Add((view, drag, data));
                return;
            }
        
            var first = _staged[0];
            var isPair = _rules.IsValidPair(first.data, data);

            if (!isPair)
            {
                drag.ReturnToOrigin();
                return;
            }
        
            drag.AcceptDrop(dropContent);
            PlayDropFlip(view.transform);
            PlayScaleUp(view.transform);
            _staged.Add((view, drag, data));
        
            _controller.OnCardSelected(first.data);
            _controller.OnCardSelected(data);
        
            MoveToPile(first.view);
            MoveToPile(view);
        
            _staged.Clear();
        }

        private void PlayDropFlip(Transform t)
        {
            t.DOKill();
            t.localRotation = Quaternion.identity;
            t.DOLocalRotate(new Vector3(0f, 0f, 45f), 0.30f);
        }
        
        private void PlayScaleUp(Transform card)
        {
            if (!card) return;

            card.DOKill();
            card.DOScale(1.3f, 0.25f).SetEase(Ease.OutElastic);
        }

        private void MoveToPile(CardView view)
        {
            if (!view) return;
        
            var drag = view.GetComponent<CardDragHandler>();
            if (drag) drag.enabled = false;

            var cg = view.GetComponent<CanvasGroup>();
            if (!cg) cg = view.gameObject.AddComponent<CanvasGroup>();
            cg.blocksRaycasts = false;
            cg.interactable = false;
        
            view.transform.SetParent(matchedPile, worldPositionStays: false);
        
            var targetPos = new Vector3(pileStep.x * _pileCount, pileStep.y * _pileCount, 0f);
            var rotZ = Random.Range(-pileFanAngle, pileFanAngle);
        
            view.transform.DOKill();
            view.transform.DOLocalMove(targetPos, pileAnimDuration).SetEase(Ease.OutQuad);
            view.transform.DOLocalRotate(new Vector3(0, 0, rotZ), pileAnimDuration).SetEase(Ease.OutQuad);
            view.transform.DOScale(pileScale, 0.25f).SetEase(Ease.OutQuad);

        
            view.transform.SetAsLastSibling();
            _pileCount++;
        }
    }
}
