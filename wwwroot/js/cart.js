function addToCart(productId, quantity) {
    $.ajax({
        url: "/Cart/AddToCart",
        type: "POST",
        data: { productId: productId, quantity: quantity },
        success: function (result) {
            if (result.success) {
                alert("Sản phẩm đã được thêm vào giỏ hàng!");
            }
        },
        error: function () {
            alert("Đã có lỗi xảy ra khi thêm sản phẩm vào giỏ hàng!");
        }
    });
}

function updateCart(productId, _this) {
    $.ajax({
        url: "/Cart/UpdateCart",
        type: "POST",
        data: { productId: productId, quantity: _this.value },
        success: function (result) {

            // tìm thẻ td chứa giá trị price trong hàng sản phẩm
            let priceCell = _this.parentElement.previousElementSibling;
            // lấy giá trị price từ thẻ td
            let price = parseInt(priceCell.textContent);
            // tính toán giá trị mới cho cột "Total"
            let total = _this.value * price;    
            // tìm thẻ td chứa giá trị "Total" trong hàng sản phẩm và cập nhật giá trị mới
            let totalCell = _this.parentElement.nextElementSibling;
            totalCell.textContent = total.toFixed(2); // làm tròn đến 2 chữ số sau dấu thập phân
        },
        error: function () {
            
        }
    });
}