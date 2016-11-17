using DG.Tweening;
using UnityEngine;

namespace EA4S.Assessment
{
    public class DroppableBehaviour : MonoBehaviour, IDroppable
    {
        IDragManager dragManager = null;
        Tween tween = null;

        public void SetDragManager( IDragManager dragManager)
        {
            this.dragManager = dragManager;
        }

        Vector3 origin; // Memorize starting position for going back
        void OnMouseDown()
        {
            if (!dragEnabled)
                return;

            // If I place an LL above another one, then the other one should fall down
            // So when I click a LL that is linked I keep its original position
            if(GetLinkedPlaceholder()==null)
                origin = transform.localPosition;

            dragManager.StartDragging( this);
            SetScale( 1.3f);
        }

        void SetScale( float scale)
        {
            if (tween != null)
                tween.Kill( false);

            tween = transform.DOScale( scale, 0.4f).OnComplete( () => tween = null);
        }

        void OnMouseUp()
        {
            if (!dragEnabled)
                return;

            dragManager.StopDragging( this);
        }

        bool dragEnabled = false;
        public void Disable()
        {
            dragEnabled = false;
        }

        public void Enable()
        {
            dragEnabled = true;
        }

        public void StartDrag()
        {
            SetScale( 1.3f);
        }

        public void StopDrag()
        {
            SetScale( 1f);
        }


        PlaceholderBehaviour linkedBehaviour = null;
        public void LinkToPlaceholder( PlaceholderBehaviour behaviour)
        {
            linkedBehaviour = behaviour;
            if (behaviour.LinkedDroppable != null)
                behaviour.LinkedDroppable.Detach();

            // Link this answer to placeholder
            transform.localPosition = behaviour.transform.localPosition;
            linkedBehaviour = behaviour;
            behaviour.LinkedDroppable = this;
        }

        public void Detach( bool jumpBack = true)
        {
            if(jumpBack)
                transform.DOLocalMove( origin, 0.7f).SetEase( Ease.OutBounce);

            if(linkedBehaviour != null)
                linkedBehaviour.LinkedDroppable = null;
            linkedBehaviour = null;
        }

        public Transform GetTransform()
        {
            return transform;
        }

        public PlaceholderBehaviour GetLinkedPlaceholder()
        {
            return linkedBehaviour;
        }
    }
}
