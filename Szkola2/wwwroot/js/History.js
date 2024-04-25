function Delete(url) {
    swal({
        title: "Wciśnij OK jeżeli chcesz trwale ususnąć grafikę",
        text: "Jeżeli wciśniesz OK, grafika zostanie trwale usunięta",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then((willDelete) => {
        if (willDelete == true) {
            $.ajax({
                type: "DELETE",
                url: url,
                success: function (data) {
                    if (data.success) {
                        toastr.info(data.message);
                        setTimeout(reloadPage,3000);
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