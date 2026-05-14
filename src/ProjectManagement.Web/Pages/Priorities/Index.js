$(function () {
    var l = abp.localization.getResource('ProjectManagement');

    var dataTable = $('#PriorityTable').DataTable(
        abp.libs.datatables.normalizeConfiguration({
            serverSide: true,
            paging: true,
            order: [[0, 'asc']],
            searching: false,
            scrollX: true,
            processing: true,
            ajax: abp.libs.datatables.createAjax(
                projectManagement.priorities.priority.getList,
                function () {
                    return {
                        filter: $('#PrioritySearch').val(),
                    };
                },
            ),
            columnDefs: [
                {
                    title: l('Actions'),
                    width: '5%',
                    rowAction: {
                        items: [
                            {
                                text: l('Edit'),
                                visible: abp.auth.isGranted('ProjectManagement.Priorities.Edit'),
                                action: function (data) {
                                    editPriority(data.record.id);
                                },
                            },
                            {
                                text: l('Delete'),
                                visible: abp.auth.isGranted('ProjectManagement.Priorities.Delete'),
                                confirmMessage: function (data) {
                                    return l('PriorityDeleteConfirmationMessage');
                                },
                                action: function (data) {
                                    projectManagement.priorities.priority
                                        .delete(data.record.id)
                                        .then(function () {
                                            abp.notify.success(
                                                l('DeletedSuccessfully'),
                                            );
                                            dataTable.ajax.reload();
                                        });
                                },
                            },
                        ],
                    },
                },
                {
                    title: l('Title'),
                    data: 'title',
                    width: '20%',
                    defaultContent: '',
                },
            ],
        }),
    );

    // Store table reference globally so modals can access it
    window.projectTable = dataTable;

    var createModal = new abp.ModalManager(
        abp.appPath + 'Priorities/CreateModal',
    );
    var editModal = new abp.ModalManager(abp.appPath + 'Priorities/EditModal');

    createModal.onResult(function () {
        dataTable.ajax.reload();
    });

    editModal.onResult(function () {
        dataTable.ajax.reload();
    });

    document.addEventListener('priorityCreated', function () {
        dataTable.ajax.reload();
    });

    $('#NewPriorityButton').click(function (e) {
        e.preventDefault();
        createModal.open();
    });

    $('#PrioritySearchButton').on('click', function () {
        lastSearchValue = ($('#PrioritySearch').val() || '').trim();
        dataTable.ajax.reload();
    });

    $('#PrioritySearch').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault();
            lastSearchValue = ($('#PrioritySearch').val() || '').trim();
            dataTable.ajax.reload();
        }
    });
    $('#PrioritySearch').on('input search', function () {
        var currentValue = ($(this).val() || '').trim();
        if (lastSearchValue !== '' && currentValue === '') {
            dataTable.ajax.reload();
        }
        lastSearchValue = currentValue;
    });

    function editPriority(id) {
        editModal.open({ id: id });
    }
});
