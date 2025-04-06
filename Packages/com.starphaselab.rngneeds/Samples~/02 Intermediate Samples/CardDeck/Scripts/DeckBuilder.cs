using System.Collections.Generic;
using UnityEngine;

namespace RNGNeeds.Samples.CardDeck
{
    [CreateAssetMenu(fileName = "New Deck Builder", menuName = "RNGNeeds/Card Decks/Deck Builder")]
    public class DeckBuilder : ScriptableObject
    {
        public PLCollection<CardCollection> cardCollections;
        public bool clearDeckBeforeFill;
        public int maxCards;
        public CardDeck deckToFill;

        public void FillDeck()
        {
            if (clearDeckBeforeFill) deckToFill.cards.ClearList();
            deckToFill.cards.IsDepletable = true;
            
            var pickedCollections = cardCollections.PickValuesFromAll();
            var pickedCards = new List<Card>();
            
            foreach (var pickedCollection in pickedCollections)
            {
                pickedCards.AddRange(pickedCollection.cards.PickValues());
            }
            
            while (pickedCards.Count > maxCards)
            {
                pickedCards.RemoveAt(pickedCards.Count - 1);
            }
            
            foreach (var pickedCard in pickedCards)
            {
                if(deckToFill.cards.TryGetProbabilityItem(pickedCard, out var existingItem))
                {
                    existingItem.MaxUnits++;
                    continue;
                }
                
                deckToFill.cards.AddItem(new ProbabilityItem<Card>(pickedCard, 1f));
            }
            
            deckToFill.cards.RefillItems();
            deckToFill.cards.NormalizeProbabilities();
        }
    }
}