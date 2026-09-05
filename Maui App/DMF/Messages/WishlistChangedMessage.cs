using CommunityToolkit.Mvvm.Messaging.Messages;

namespace DMF.Messages
{
    // Broadcast whenever a car's wishlist (favourite) state changes anywhere in
    // the app, so every screen showing that car — Home, Wishlist, Car Detail —
    // stays in sync. Fixes: removing a car from the Wishlist screen used to leave
    // its heart still filled on the Home screen.
    public class WishlistChangedMessage : ValueChangedMessage<(int CarId, bool IsWishlisted)>
    {
        public WishlistChangedMessage(int carId, bool isWishlisted)
            : base((carId, isWishlisted)) { }
    }
}
