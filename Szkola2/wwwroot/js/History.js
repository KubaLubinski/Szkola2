function Delete(url) {
    swal({
        title: "Wciśnij OK jeżeli chcesz trwale ususnąć wpis",
        text: "Jeżeli wciśniesz OK, wpis zostanie trwale usunięty",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then((willDelete) => {
        if (willDelete) {
            $.ajax({
                type: "DELETE",
                url: url,
                success: function (data) {
                    if (data.success) {
                        toastr.success(data.message);
                        reloadPage();
                    }
                    else {
                        toastr.error(data.message);
                    }
                }
            })
        }
    });
}

function reloadPage() {
    window.location.reload();
}