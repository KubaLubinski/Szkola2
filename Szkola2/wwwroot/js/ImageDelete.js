function Delete(url) {
    swal({
        title: "Wciœnij OK je¿eli chcesz trwale ususn¹æ grafikê",
        text: "Je¿eli wciœniesz OK, grafika zostanie trwale usuniêta",
        icon: "warning",
        buttons: true,
        dangerMode: true
    }).then((willDelete) => {
        if (willDelete == true) {
            setTimeout(reloadPage, 1000);
            $.ajax({
                type: "DELETE",
                url: url,
                success: function (data) {
                    if (data.success) {
                        toastr.info(data.message);
                        setTimeout(reloadPage, 3000);
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